# Capability: Process Helper

## Purpose
Provides extension methods on `System.Diagnostics.Process` to read standard output
and standard error streams and to expose the current process priority class.

## Requirements

### Requirement: Read Process Outputs Synchronously
`ProcessHelper.WaitProcessOutputs(this Process)` SHALL read both `stdout` and `stderr`
completely and then wait for the process to exit, returning a `ProcessOutputs` record.

The implementation MUST read both streams in parallel to avoid deadlocking when the
process fills the OS pipe buffer for one stream while the other stream is being read.

#### Scenario: Normal process completion
- **WHEN** a process writes to both stdout and stderr and then exits
- **THEN** `WaitProcessOutputs` returns a `ProcessOutputs` with both streams fully captured

#### Scenario: Large stderr output
- **WHEN** a process writes more data to stderr than the OS pipe buffer can hold
  while simultaneously writing to stdout
- **THEN** `WaitProcessOutputs` does NOT deadlock and returns complete output from both streams

### Requirement: Read Process Outputs Asynchronously
`ProcessHelper.WaitProcessOutputsAsync(this Process)` SHALL read both streams concurrently
using `Task.WhenAll` before calling `WaitForExitAsync`.

#### Scenario: Concurrent stream reading
- **WHEN** called on a running process
- **THEN** both `StandardOutput.ReadToEndAsync()` and `StandardError.ReadToEndAsync()` are
  awaited concurrently via `Task.WhenAll`, not sequentially

### Requirement: Current Process Priority
`ProcessHelper.CurrentProcessPriorityClass()` SHALL return the `ProcessPriorityClass`
of the currently executing process without leaking process handles.

#### Scenario: Returns current priority
- **WHEN** called
- **THEN** returns the `ProcessPriorityClass` of `Process.GetCurrentProcess()`
