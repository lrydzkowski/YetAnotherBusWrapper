# The library is configuration agnostic

The library never references `IConfiguration` and reads no files or
configuration sections. It defines its own options types, reads them through
`IOptionsMonitor<T>` with one named instance per logical entity name, and
returns an `OptionsBuilder<T>` from each registration. The host chooses how
to fill the options: `.Bind(section)`, `.Configure(...)`, or environment
variables. Options are validated with `ValidateOnStart`.

## Consequences

- Shared values such as a connection string are set by binding two sections
  in the host, not by a library convention.
- The instance suffix still reads `WEBSITE_INSTANCE_ID` and the machine name
  through a replaceable `IInstanceNameProvider`. These are environment
  lookups, not configuration.
