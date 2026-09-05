import { readFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const DIRECT_DEPENDENCY_TYPES = ['dependencies', 'devDependencies'];

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const frontendDirectory = path.resolve(scriptDirectory, '..');

const options = parseOptions(process.argv.slice(2));
const packageJsonPath = resolveConfiguredPath(options.packageJson, path.join(frontendDirectory, 'package.json'));
const policyJsonPath = path.resolve(
  process.cwd(),
  options.policyJson ?? path.relative(process.cwd(), path.join(frontendDirectory, 'dependency-policy.json')),
);

const failures = [];

const [packageJson, policy] = await Promise.all([
  readJsonFile(packageJsonPath, 'package.json'),
  readJsonFile(policyJsonPath, 'dependency policy'),
]);

validatePolicyShape(policy, failures);
validatePackageDependencySections(packageJson, failures);

if (failures.length === 0) {
  validateDirectDependencies(packageJson, policy, failures);
}

if (failures.length > 0) {
  console.error('Dependency policy check failed:');
  for (const failure of failures) {
    console.error(`- ${failure}`);
  }
  process.exitCode = 1;
} else {
  console.log('Dependency policy check passed.');
  console.log('Approved direct dependencies match frontend/dependency-policy.json.');
  console.log('Transitive dependencies are intentionally excluded from this direct dependency guard.');
}

function parseOptions(args) {
  const parsed = {};

  for (let index = 0; index < args.length; index += 1) {
    const argument = args[index];

    if (argument === '--package-json') {
      parsed.packageJson = readOptionValue(argument, args, index);
      index += 1;
      continue;
    }

    if (argument.startsWith('--package-json=')) {
      parsed.packageJson = argument.slice('--package-json='.length);
      continue;
    }

    if (argument === '--policy-json') {
      parsed.policyJson = readOptionValue(argument, args, index);
      index += 1;
      continue;
    }

    if (argument.startsWith('--policy-json=')) {
      parsed.policyJson = argument.slice('--policy-json='.length);
      continue;
    }

    throw new Error(`Unknown argument: ${argument}`);
  }

  return parsed;
}

function readOptionValue(optionName, args, optionIndex) {
  const value = args[optionIndex + 1];
  if (!value || value.startsWith('--')) {
    throw new Error(`Missing value for ${optionName}.`);
  }

  return value;
}

function resolveConfiguredPath(configuredPath, defaultPath) {
  if (configuredPath === undefined) {
    return defaultPath;
  }

  return path.resolve(process.cwd(), configuredPath);
}

async function readJsonFile(filePath, label) {
  let raw;
  try {
    raw = await readFile(filePath, 'utf8');
  } catch (error) {
    throw new Error(`Unable to read ${label} at ${filePath}: ${error.message}`);
  }

  try {
    return JSON.parse(raw);
  } catch (error) {
    throw new Error(`Unable to parse ${label} at ${filePath}: ${error.message}`);
  }
}

function validatePolicyShape(policy, failures) {
  if (!isPlainObject(policy)) {
    failures.push('dependency policy must be a JSON object.');
    return;
  }

  if (policy.schemaVersion !== 1) {
    failures.push('dependency policy schemaVersion must be 1.');
  }

  if (!isPlainObject(policy.approvedDirectDependencies)) {
    failures.push('dependency policy approvedDirectDependencies must be an object.');
    return;
  }

  for (const dependencyType of DIRECT_DEPENDENCY_TYPES) {
    const approvedDependencies = policy.approvedDirectDependencies[dependencyType];
    if (!isPlainObject(approvedDependencies)) {
      failures.push(`dependency policy approvedDirectDependencies.${dependencyType} must be an object.`);
      continue;
    }

    validateDependencyMap(
      approvedDependencies,
      `dependency policy approvedDirectDependencies.${dependencyType}`,
      failures,
    );
  }

  for (const key of Object.keys(policy.approvedDirectDependencies)) {
    if (!DIRECT_DEPENDENCY_TYPES.includes(key)) {
      failures.push(`dependency policy contains unsupported approved direct dependency section: ${key}.`);
    }
  }

  if (!isPlainObject(policy.deniedDirectDependencies)) {
    failures.push('dependency policy deniedDirectDependencies must be an object.');
  } else {
    validateDependencyMap(policy.deniedDirectDependencies, 'dependency policy deniedDirectDependencies', failures);
    validateApprovedAndDeniedDependenciesAreDisjoint(policy, failures);
  }

  validateNoCrossSectionDuplicates(policy.approvedDirectDependencies, 'dependency policy', failures);
}

function validatePackageDependencySections(packageJson, failures) {
  if (!isPlainObject(packageJson)) {
    failures.push('package.json must be a JSON object.');
    return;
  }

  for (const dependencyType of DIRECT_DEPENDENCY_TYPES) {
    const dependencies = packageJson[dependencyType];
    if (dependencies === undefined) {
      continue;
    }

    if (!isPlainObject(dependencies)) {
      failures.push(`package.json ${dependencyType} must be an object when present.`);
      continue;
    }

    validateDependencyMap(dependencies, `package.json ${dependencyType}`, failures);
  }

  for (const dependencyType of ['optionalDependencies', 'peerDependencies', 'bundledDependencies', 'bundleDependencies']) {
    if (packageJson[dependencyType] !== undefined) {
      failures.push(`package.json ${dependencyType} is not allowed by the direct dependency guard.`);
    }
  }

  validateNoCrossSectionDuplicates(packageJson, 'package.json', failures);
}

function validateDirectDependencies(packageJson, policy, failures) {
  const deniedDirectDependencies = policy.deniedDirectDependencies;

  for (const dependencyType of DIRECT_DEPENDENCY_TYPES) {
    const actualDependencies = packageJson[dependencyType] ?? {};
    const approvedDependencies = policy.approvedDirectDependencies[dependencyType];

    for (const [packageName, declaredSpecifier] of Object.entries(actualDependencies)) {
      if (Object.hasOwn(deniedDirectDependencies, packageName)) {
        failures.push(
          `${dependencyType}.${packageName} is explicitly denied: ${deniedDirectDependencies[packageName]}`,
        );
        continue;
      }

      if (!Object.hasOwn(approvedDependencies, packageName)) {
        const approvedType = findApprovedDependencyType(policy, packageName);
        if (approvedType) {
          failures.push(
            `${dependencyType}.${packageName} is approved only in ${approvedType}; dependency category movement is not allowed.`,
          );
        } else {
          failures.push(`${dependencyType}.${packageName} is not approved by dependency-policy.json.`);
        }
        continue;
      }

      const approvedSpecifier = approvedDependencies[packageName];
      if (declaredSpecifier !== approvedSpecifier) {
        failures.push(
          `${dependencyType}.${packageName} specifier changed from approved ${JSON.stringify(approvedSpecifier)} to ${JSON.stringify(declaredSpecifier)}.`,
        );
      }
    }

    for (const [packageName, approvedSpecifier] of Object.entries(approvedDependencies)) {
      if (!Object.hasOwn(actualDependencies, packageName)) {
        const actualType = findActualDependencyType(packageJson, packageName);
        if (actualType) {
          failures.push(
            `${packageName} moved from approved ${dependencyType} to ${actualType}; dependency category movement is not allowed.`,
          );
        } else {
          failures.push(`${dependencyType}.${packageName} with specifier ${JSON.stringify(approvedSpecifier)} is missing from package.json.`);
        }
      }
    }
  }
}

function validateDependencyMap(dependencies, label, failures) {
  for (const [packageName, declaredSpecifier] of Object.entries(dependencies)) {
    if (typeof packageName !== 'string' || packageName.trim() === '') {
      failures.push(`${label} contains an empty package name.`);
    }

    if (typeof declaredSpecifier !== 'string' || declaredSpecifier.trim() === '') {
      failures.push(`${label}.${packageName} must have a non-empty string specifier.`);
    }
  }
}

function validateNoCrossSectionDuplicates(source, label, failures) {
  if (!isPlainObject(source.dependencies) || !isPlainObject(source.devDependencies)) {
    return;
  }

  const dependencyNames = new Set(Object.keys(source.dependencies));
  for (const packageName of Object.keys(source.devDependencies)) {
    if (dependencyNames.has(packageName)) {
      failures.push(`${label} lists ${packageName} in both dependencies and devDependencies.`);
    }
  }
}

function validateApprovedAndDeniedDependenciesAreDisjoint(policy, failures) {
  const deniedDependencyNames = new Set(Object.keys(policy.deniedDirectDependencies));

  for (const dependencyType of DIRECT_DEPENDENCY_TYPES) {
    for (const packageName of Object.keys(policy.approvedDirectDependencies[dependencyType])) {
      if (deniedDependencyNames.has(packageName)) {
        failures.push(
          `dependency policy lists ${packageName} in both approvedDirectDependencies.${dependencyType} and deniedDirectDependencies.`,
        );
      }
    }
  }
}

function findApprovedDependencyType(policy, packageName) {
  return DIRECT_DEPENDENCY_TYPES.find((dependencyType) =>
    Object.hasOwn(policy.approvedDirectDependencies[dependencyType], packageName),
  );
}

function findActualDependencyType(packageJson, packageName) {
  return DIRECT_DEPENDENCY_TYPES.find((dependencyType) =>
    isPlainObject(packageJson[dependencyType]) && Object.hasOwn(packageJson[dependencyType], packageName),
  );
}

function isPlainObject(value) {
  return value !== null && typeof value === 'object' && !Array.isArray(value);
}
