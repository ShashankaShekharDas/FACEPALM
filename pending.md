# Pending Tasks
Last updated: 2025-08-12

- [ ] Support More Providers
  - Description: Extend the provider abstraction to integrate additional external providers.
  - Subtasks:
    - Define/confirm provider interface and capability matrix.
    - Add unit/integration tests and mocks for provider APIs.
    - Update docs and examples.
  - Acceptance Criteria:
    - At least two new providers are selectable via configuration.
    - Tests cover happy-path and failure scenarios.
    - Documentation includes setup, config, and usage notes.

- [ ] Automated File Checker (Periodic Integrity Verification)
  - Description: Periodically verify file integrity and report anomalies.
  - Subtasks:
    - Choose hashing strategy (e.g., SHA-256) and baseline generation.
    - Implement scheduler (Windows Task Scheduler, cron, or hosted service).
    - Implement scan, diff, and reporting (console, log, optional alert hook).
    - Add configuration for paths, exclusions, schedule, and performance limits.
    - Add tests for large sets, locked files, and transient errors.
  - Acceptance Criteria:
    - Configurable schedule and target paths.
    - Reports changed/missing/corrupted files with clear exit codes.
    - Logs suitable for troubleshooting; handles large directories reliably.

- [ ] CLI Tool for Easy Access
  - Description: Provide a cross-platform CLI for common operations.
  - Subtasks:
    - Scaffold CLI (e.g., System.CommandLine) with commands and help.
    - Core commands: init, verify, providers list/add/remove, config set/get.
    - Implement structured output (text/json) and proper exit codes.
    - Package as a .NET tool and provide installation instructions.
    - Add unit/integration tests, usage examples, and completion scripts.
  - Acceptance Criteria:
    - CLI installs and runs via a single command.
    - --help shows clear usage; commands return correct exit codes.
    - Works on Windows, Linux, and macOS.