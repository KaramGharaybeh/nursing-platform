import assert from "node:assert/strict";
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";
import test from "node:test";

const validatorPath = new URL("./validate-delegation-packet.mjs", import.meta.url);

function validPacket() {
  return {
    PACKET_ID: "TEST-001",
    TASK_LABEL: "validator test",
    TASK_PURPOSE: "Prove packet validation",
    "TASK_TYPE / DOMAIN": "GOVERNANCE",
    RISK_LEVEL: "LOW",
    ACCURACY_REQUIREMENT: "STANDARD",
    ASSIGNED_ROLE: "independent-verifier",
    ASSIGNED_MODEL: "opencode/mimo-v2.6-flash-free",
    FALLBACK_MODELS: [],
    REPOSITORY_SNAPSHOT: "temporary test fixture",
    WORKING_TREE_EXPECTATION: "isolated temporary workspace",
    ALLOWED_FILES: ["README.md"],
    FORBIDDEN_FILES: ["src/**"],
    GLOBAL_CONTEXT_MODULES: ["AGENTS.md"],
    TASK_CONTEXT_MODULES: ["README.md"],
    CONTEXT_SELECTION_RATIONALE: "Only the test packet is in scope",
    SKILLS_TO_EVALUATE: ["using-superpowers"],
    KNOWN_CONSTRAINTS: ["Do not edit source"],
    REQUIREMENTS: ["Validate required fields"],
    ACCEPTANCE_CRITERIA: ["Complete packet passes"],
    VERIFICATION_REQUIRED: ["Run the validator test"],
    FORBIDDEN_ASSUMPTIONS: ["Do not infer missing fields"],
    STOP_CONDITIONS: ["Missing required packet data"],
    "SCOPE / BUDGET": "One fixture",
    RESULT_SHAPE: ["STATUS", "EVIDENCE"],
  };
}

function runValidator(packet) {
  const temporaryRoot = path.join(os.tmpdir(), "opencode");
  mkdirSync(temporaryRoot, { recursive: true });
  const directory = mkdtempSync(path.join(temporaryRoot, "nps-packet-validator-"));
  const packetPath = path.join(directory, "packet.json");
  writeFileSync(packetPath, JSON.stringify(packet), "utf8");
  const result = spawnSync(process.execPath, [validatorPath.pathname, packetPath], {
    encoding: "utf8",
  });
  rmSync(directory, { recursive: true, force: true });
  return result;
}

test("complete canonical packet passes validation", () => {
  const result = runValidator(validPacket());
  assert.equal(result.status, 0, result.stderr || result.stdout);
  assert.match(result.stdout, /PACKET_VALID requiredFields=25/);
});

test("missing required acceptance criteria is rejected", () => {
  const packet = validPacket();
  delete packet.ACCEPTANCE_CRITERIA;
  const result = runValidator(packet);
  assert.notEqual(result.status, 0);
  assert.match(`${result.stdout}${result.stderr}`, /ACCEPTANCE_CRITERIA/);
});

test("roadmap task identifier can replace task label", () => {
  const packet = validPacket();
  delete packet.TASK_LABEL;
  packet.TASK_ID = "T-TEST-001";
  const result = runValidator(packet);
  assert.equal(result.status, 0, result.stderr || result.stdout);
});

test("packet without task identifier or label is rejected", () => {
  const packet = validPacket();
  delete packet.TASK_LABEL;
  const result = runValidator(packet);
  assert.notEqual(result.status, 0);
  assert.match(`${result.stdout}${result.stderr}`, /TASK_ID.*TASK_LABEL/);
});
