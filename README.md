# GalArc

[![](https://github.com/detached64/GalArc/actions/workflows/build.yml/badge.svg)](https://github.com/detached64/GalArc/actions/workflows/build.yml) [![](https://img.shields.io/github/license/detached64/GalArc)](./LICENSE) [![](https://img.shields.io/github/v/release/detached64/GalArc?include_prereleases)](https://github.com/detached64/GalArc/releases/latest)

Galgame Archive Tool.

Mainly focus on the unpacking and repacking of galgame archives.

## Download

[Releases](https://github.com/detached64/GalArc/releases/latest)

Get ci builds [here](https://github.com/detached64/GalArc/actions/workflows/build.yml).

## Build

```
dotnet publish -c Release -r win-x64
```

### Cross-Platform Compatibility

Technically, the program compilable and executable on Windows, Linux, and macOS through the `dotnet publish` command. However, it has only been tested on Windows 11 x64. Platform-specific variations in filesystem implementations and underlying architectural differences may lead to unexpected behaviors on non-Windows operating systems.

If you encounter any issues while running the program on non-Windows platforms, feel free to report them.

## Contributing

Contributions are welcome! Please refer to the [Contribution Guide](./docs/contribution.md) for details.

## License

This project is licensed under the GNU General Public License v3.0. See the [LICENSE](./LICENSE) file for details.

## Third-Party Licenses

This project uses some third-party libraries. Please refer to the [3RD_PARTY_LICENSES](./3RD_PARTY_LICENSES.md) file for details.
