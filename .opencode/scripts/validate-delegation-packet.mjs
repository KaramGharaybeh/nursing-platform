#!/usr/bin/env node

import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../..");
const orchestrationContractPath = path.join(
  repositoryRoot,
  "docs/development/model-orchestration.md",
);

function getPacketFieldNames(contract) {
  const sectionStart = contract.indexOf("## Mandatory Delegation Packet");
  if (sectionStart < 0) {
    throw new Error("Canonical Mandatory Delegation Packet section was not found");
  }

  const afterStart = contract.slice(sectionStart);
  const nextSection = afterStart.indexOf("\n## ", "## Mandatory Delegation Packet".length);
  const section = nextSection < 0 ? afterStart : afterStart.slice(0, nextSection);
  const fields = [];

  for (const line of section.split(/\r?\n/)) {
    if (/^-\s+`TASK_ID`\s+\(or\s+`TASK_LABEL`\s/.test(line)) {
      fields.push("TASK_ID or TASK_LABEL");
      continue;
    }

    const match = line.match(/^-\s+`([A-Z][A-Z0-9_]*(?:\s*\/\s*[A-Z][A-Z0-9_]*)?)`(?:\s|$)/);
    if (match) fields.push(match[1].replace(/\s+/g, " "));
  }

  if (fields.length === 0) {
    throw new Error("Canonical packet fields could not be read");
  }

  return fields;
}

function hasValue(value) {
  if (value === undefined || value === null) return false;
  return typeof value !== "string" || value.trim().length > 0;
}

export function validatePacket(packet, contract) {
  if (!packet || typeof packet !== "object" || Array.isArray(packet)) {
    return { valid: false, missing: ["packet object"] };
  }

  const requiredFields = getPacketFieldNames(contract);
  const hasTaskId = hasValue(packet.TASK_ID);
  const hasTaskLabel = hasValue(packet.TASK_LABEL);
  const missing = requiredFields.filter((field) => {
    if (field === "TASK_ID or TASK_LABEL") return !hasTaskId && !hasTaskLabel;
    return !hasValue(packet[field]);
  });

  return { valid: missing.length === 0, missing, requiredCount: requiredFields.length };
}

async function main() {
  const packetPath = process.argv[2];
  if (!packetPath) {
    process.stderr.write("Usage: node validate-delegation-packet.mjs <packet.json>\n");
    process.exitCode = 2;
    return;
  }

  try {
    const [packetText, contract] = await Promise.all([
      readFile(packetPath, "utf8"),
      readFile(orchestrationContractPath, "utf8"),
    ]);
    const packet = JSON.parse(packetText);
    const result = validatePacket(packet, contract);

    if (result.valid) {
      process.stdout.write(`PACKET_VALID requiredFields=${result.requiredCount}\n`);
      return;
    }

    process.stderr.write(`PACKET_INVALID missing=${result.missing.join(",")}\n`);
    process.exitCode = 1;
  } catch (error) {
    process.stderr.write(`PACKET_INVALID ${error instanceof SyntaxError ? "invalid JSON" : error.message}\n`);
    process.exitCode = 1;
  }
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  await main();
}
