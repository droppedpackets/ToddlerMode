# ToddlerMode

## Child-Safe Keyboard Restrictor for Windows

ToddlerMode is a simple Windows application that creates a safe environment for toddlers to use a computer by restricting keyboard input that could disrupt their experience or damage the system.

![ToddlerMode Screenshot](toddler_mode_icon.svg?raw=true)

## Features

- **Restrict System Shortcuts**: Blocks potentially disruptive key combinations (Alt+Tab, Windows key, Alt+F4, etc.)
- **Full-Screen Mode**: Keeps the active application full-screen and in focus
- **Allow Normal Typing**: Regular typing works normally within the current application
- **Parent Exit Mechanisms**: Simple escape routes for adults
  - Ctrl+Alt+Delete (system-level shortcut)
  - Ctrl+Alt+Escape (built-in escape shortcut)

## Why ToddlerMode?

Toddlers love pressing keys on keyboards, but they can easily:

- Close applications accidentally
- Switch windows unintentionally
- Access system functions
- Trigger unwanted key combinations

ToddlerMode prevents these issues while still allowing normal interaction with the current application.

## Installation

### Option 1: Download Pre-built Binary

1. Go to the [Releases](https://github.com/droppedpackets/ToddlerMode/releases) page
2. Download the latest `.exe` file
3. Run the application (requires administrator privileges)

### Option 2: Build from Source

1. Ensure you have .NET 6.0 SDK or later installed
2. Clone this repository:
   ```
   git clone https://github.com/yourusername/ToddlerMode.git
   ```
3. Navigate to the project directory:
   ```
   cd ToddlerMode
   ```
4. Build the application:
   ```
   dotnet build
   ```
5. Run the application:
   ```
   dotnet run
   ```

## Usage

1. Launch ToddlerMode
2. Click "Activate ToddlerMode"
3. The application will go full-screen and keyboard restrictions will be enabled
4. To exit ToddlerMode:
   - Press Ctrl+Alt+Delete
   - Press Ctrl+Alt+Escape

## System Requirements

- Windows 10 or later
- .NET 6.0 Runtime or later
- Administrator privileges (required for keyboard hook functionality)

## Technical Details

ToddlerMode works by:

- Setting up a low-level keyboard hook to intercept key combinations
- Applying a whitelist/blacklist approach to key presses
- Making the application window fullscreen and topmost
- Hiding UI elements that could be accidentally triggered

## Contributing

Contributions are welcome! Feel free to:

- Report bugs
- Suggest features
- Submit pull requests

Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with C# and WPF
- Uses Windows low-level keyboard hooks

---

Created with ❤️ for frustrated parents everywhere
