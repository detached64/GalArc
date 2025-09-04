# GalArc

Galgame Archive Tool.

Mainly focus on the unpacking and repacking of galgame archives.

## Download

[Releases](https://github.com/detached64/GalArc/releases/latest)

Get ci builds [here](https://github.com/detached64/GalArc/actions).

## Build

```
dotnet publish -c Release -r win-x64
```

### Cross-Platform Compatibility

Technically, the program can be compiled and run on Windows, Linux, and macOS through `dotnet publish`.

However, the program is only tested on Windows. Some behaviors on other platforms may not be as expected because of the differences in file systems and other factors.

If you encounter any issues while running the program on non-Windows platforms, feel free to report them.

## Contributing

Contributions are welcome! Please refer to the [contribution guide](./docs/contribution.md) for details.

## License

GNU General Public License v3.0
