# KeyboardHookPoc
A basic keyboard hook in C# using SetWindowsHookEx.

## Overview
This project explores low-level keyboard hooking in C# without relying on Windows Forms or the Application.Run method, as typically seen in many keyloggers, like Quasar Rat. The motivation behind this project is to learn more about hooking techniques while keeping the payload size smaller and avoiding the need for DLL or EXE injection.

## Description
The `KeyboardHookPoc` demonstrates how to implement a low-level keyboard hook by utilizing the `SetWindowsHookEx` function. By adding a `GetMessage` loop, the code captures keyboard input without relying on Windows Forms. The self-contained .NET feature is utilized to keep the project streamlined and efficient.

Please note that this is a proof-of-concept and can be improved further to meet specific requirements and use cases.

Feel free to modify and explore the code for your learning purposes.
Always ensure to comply with ethical guidelines and applicable laws when working with any form of system-level interception or monitoring.

## Usage
1. Clone the repository.
2. Build the project.
3. Run the generated executable.

## Contributions
Contributions to improve this project are welcome! Feel free to submit pull requests or open issues for discussion.

## License
This project is licensed under the [MIT License](LICENSE).

