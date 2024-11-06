# CustomSftpTool

**CustomSftpTool** is a powerful command-line tool designed to manage deployment profiles and automate deployments via SSH and SFTP of .NET projects hosten on Linux servers. With this tool, you can effortlessly add, edit, list, and deploy profiles, streamlining your deployment workflow.

---

## 📋 Table of Contents

1. [Features](#features)
2. [Prerequisites](#prerequisites)
3. [Installation](#installation)
4. [Building the Application](#building-the-application)
5. [Publishing as a Standalone Executable](#publishing-as-a-standalone-executable)
6. [Adding `customSFTP` to the System PATH](#adding-customssh-to-the-system-path)
7. [Usage](#usage)
    - [Available Commands](#available-commands)
    - [Examples](#examples)
8. [Profile Management](#profile-management)
    - [Adding a Profile](#adding-a-profile)
    - [Editing a Profile](#editing-a-profile)
    - [Listing Profiles](#listing-profiles)
    - [Showing a Profile](#showing-a-profile)
    - [Removing a Profile](#removing-a-profile)
9. [Deployment](#deployment)
10. [Troubleshooting](#troubleshooting)
11. [Contributing](#contributing)
12. [License](#license)

---

## Features

-   **Profile Management:** Easily add, edit, list, show, and remove deployment profiles.
-   **Automated Deployment:** Deploy applications via SSH and SFTP with a single command.
-   **Logging:** Detailed logs for deployment processes.
-   **Self-contained Executable:** Run the tool without needing the .NET SDK installed.
-   **Cross-Platform Support:** Compatible with Windows, Linux, and macOS.

---

## Prerequisites

Before you begin, ensure you have met the following requirements:

-   **Operating System:** Windows 10 or later (also compatible with Linux and macOS).
-   **.NET SDK:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed.
-   **SSH Access:** Valid SSH credentials (host, username, private key) for deployment.
-   **Git:** [Git](https://git-scm.com/downloads) installed (optional, for cloning the repository).

---

## Installation

1. **Clone the Repository (Optional):**

    If you haven't already, clone the repository to your local machine:

    ```bash
    git clone https://github.com/yourusername/CustomSftpTool.git
    cd CustomSftpTool
    ```

    Alternatively, you can download the source code as a ZIP file from the repository and extract it to your desired location.

2. **Building the Application**

    To build the application from source, follow these steps:

    - Navigate to the Project Directory:

        Open a terminal or PowerShell window and navigate to the root directory of the project where the `.csproj` file is located.

        ```powershell
        cd C:\path\to\CustomSftpTool
        ```

    - Restore Dependencies:

        Restore the necessary NuGet packages.

        ```powershell
        dotnet restore
        ```

    - Build the Project:

        Build the project in Release configuration.

        ```powershell
        dotnet build -c Release
        ```

    **Output:**

    After a successful build, the binaries will be located in the `bin\Release\net8.0\` directory.

3. **Publishing as a Standalone Executable**

    To run CustomSftpTool as a standalone executable named `customSFTP`, you need to publish it as a self-contained application.

    - Set the Assembly Name (Optional):

        To ensure the executable is named `customSFTP`, update the `<AssemblyName>` in your `.csproj` file.

        ```xml
        <PropertyGroup>
          <AssemblyName>customSFTP</AssemblyName>
          <!-- Other properties -->
        </PropertyGroup>
        ```

    - Publish the Application:

        Run the `dotnet publish` command with the appropriate Runtime Identifier (RID).

        ```powershell
        dotnet publish CustomSftpTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
        ```

        **Parameters:**

        - `-c Release`: Build configuration.
        - `-r win-x64`: Target runtime. Change this based on your OS:
            - Windows x64: `win-x64`
            - Linux x64: `linux-x64`
            - macOS x64: `osx-x64`
            - macOS ARM64: `osx-arm64`
        - `--self-contained true`: Publish as a self-contained application.
        - `-p:PublishSingleFile=true`: Bundle the application into a single executable.
        - `-o ./publish`: Output directory.

    **Output:**

    After publishing, the publish directory will contain `customSFTP.exe` (on Windows) or `customSFTP` (on Linux/macOS).

4. **Adding customSFTP to the System PATH**

    Adding `customSFTP` to your system's PATH allows you to run it from any directory without specifying its full path.

    **Method 1: Adding the publish Directory to PATH**

    - Determine the Full Path:

        For example:

        ```plaintext
        C:\path\to\CustomSftpTool\publish
        ```

    - Add to PATH via PowerShell:

        ```powershell
        [Environment]::SetEnvironmentVariable("Path", $env:Path + ";C:\path\to\CustomSftpTool\publish", "User")
        ```

    - Update Current Session:

        To make the change effective immediately in the current PowerShell session:

        ```powershell
        $env:Path += ";C:\path\to\CustomSftpTool\publish"
        ```

    - Verify PATH Update:

        ```powershell
        echo $env:Path
        ```

        Ensure that `C:\path\to\CustomSftpTool\publish` is listed.

    - Restart PowerShell:

        Close and reopen PowerShell to apply the changes.

    - Run `customSFTP`:

        Now, you can run:

        ```powershell
        customSFTP --help
        ```

    **Method 2: Copying customSFTP.exe to an Existing PATH Directory**

    - Locate a PATH Directory:

        Common directories include:

        - `C:\Windows\System32`
        - `C:\Windows`
        - `C:\Program Files`

    - Copy the Executable:

        Open PowerShell as Administrator and run:

        ```powershell
        Copy-Item -Path "C:\path\to\CustomSftpTool\publish\customSFTP.exe" -Destination "C:\Windows\System32\customSFTP.exe"
        ```

    - Run `customSFTP`:

        ```powershell
        customSFTP --help
        ```

        Note: Administrative privileges are required to copy files to system directories.

---

## Usage

After successfully adding `customSFTP` to your PATH, you can use it as a native command-line tool.

### Available Commands

-   `--deploy <ProfileName>`: Deploy using a specified profile.
-   `--add-profile`: Add a new deployment profile.
-   `--list-profiles`: List all available profiles.
-   `--show-profile <ProfileName>`: Display details of a specific profile.
-   `--edit-profile <ProfileName>`: Edit an existing profile.
-   `--remove-profile <ProfileName>`: Remove a profile.
-   `--help`: Show help and usage information.

### Examples

-   Show Help:

    ```powershell
    customSFTP --help
    ```

-   Add a New Profile:

    ```powershell
    customSFTP --add-profile
    ```

    Follow the interactive prompts to enter profile details.

-   List All Profiles:

    ```powershell
    customSFTP --list-profiles
    ```

-   Show a Specific Profile:

    ```powershell
    customSFTP --show-profile "Profile Name"
    ```

-   Edit an Existing Profile:

    ```powershell
    customSFTP --edit-profile "Profile Name"
    ```

-   Remove a Profile:

    ```powershell
    customSFTP --remove-profile "Profile Name"
    ```

-   Deploy Using a Profile:

    ```powershell
    customSFTP --deploy "Profile Name"
    ```

---

## Profile Management

Managing deployment profiles is straightforward with CustomSftpTool. Each profile contains the necessary information for deploying your application.

### Adding a Profile

-   Run the Add Profile Command:

    ```powershell
    customSFTP --add-profile
    ```

-   Follow the Prompts:

    Enter the required details:

    -   Profile Name: Unique name for the profile (e.g., "Production Server").
    -   Host: SSH host address.
    -   Username: SSH username.
    -   Private Key Path: Path to your SSH private key (e.g., `C:\Users\youruser\.ssh\id_rsa`).
    -   Csproj Path: Path to your `.csproj` file.
    -   Local Directory: Directory for the build output.
    -   Remote Directory: Target directory on the remote server.
    -   Service Name: Name of the service to manage (e.g., "service-name").
    -   Excluded Files: Files or directories to exclude during deployment.

### Editing a Profile

-   Run the Edit Profile Command:

    ```powershell
    customSFTP --edit-profile "Current Profile Name"
    ```

-   Follow the Prompts:

    Update the profile details as needed. If you change the profile name, the corresponding JSON file will be renamed to maintain consistency.

### Listing Profiles

-   Run the List Profiles Command:

    ```powershell
    customSFTP --list-profiles
    ```

**Output:**

-   Available Profiles:

    -   Production Server
    -   Staging Environment
    -   Test

### Showing a Profile

-   Run the Show Profile Command:

    ```powershell
    customSFTP --show-profile "Profile Name"
    ```

**Output:**

-   Showing profile data:

    -   Name = PROFILE_NAME
    -   Host = XX.XX.XX.XX
    -   UserName = YOUR_USER_NAME
    -   PrivateKeyPath = Private_Key_Path
    -   CsprojPath = YOUR_CSPROJ_PATH
    -   LocalDir = YOUR_LOCAL_DIR_PATH
    -   RemoteDir = YOUR_REMOTE_DIR_PATH
    -   ServiceName = YOUR_SERVICE_NAME
    -   Excluded Files:
        -   EXCLUDED_FILE_1
        -   EXCLUDED_FILE_2
        -   EXCLUDED_FOLDER_1

### Removing a Profile

-   Run the Remove Profile Command:

    ```powershell
    customSFTP --remove-profile "Profile Name"
    ```

    **Example:**

    ```powershell
    customSFTP --remove-profile "Profile Name"
    ```

-   Confirmation:
    -   Profile 'Profile Name' deleted successfully.

---

## Deployment

Deploying your application using CustomSftpTool is seamless once you've set up your profiles.

### Steps to Deploy

1. **Ensure Your Profile Is Correct:**

    Verify that the profile you intend to use has all the necessary details and no errors.

    ```powershell
    customSFTP --show-profile "Profile Name"
    ```

2. **Run the Deploy Command:**

    ```powershell
    customSFTP --deploy "Profile Name"
    ```

    - Example:

    ```powershell
    customSFTP --deploy "Production Server"
    ```

# Deployment Process:

-   The tool will perform the following actions:
    -   Clean the Application: Runs dotnet clean on the specified .csproj file.
    -   Publish the Application: Runs dotnet publish to build and prepare the application for deployment.
    -   Connect to SSH: Establishes an SSH connection to the specified host.
    -   Manage Service: Checks the status of the specified service. Stops the service if it's running.
    -   Transfer Files: Uses SFTP to upload the published files to the remote directory, excluding specified files/directories.
    -   Restart Service: Starts the service again and verifies its status.

# Monitor Logs:

-   Deployment logs are saved in the logs directory. You can monitor these logs for detailed information or troubleshooting.

    ```powershell
    notepad C:\path\to\CustomSftpTool\publish\logs\deployment.log
    ```

# Troubleshooting

-   Encountering issues? Here are some common problems and their solutions.

    1. customSFTP Not Recognized

    -   Symptom:

    ```powershell
    customSFTP: The term 'customSFTP' is not recognized as a name of a cmdlet, function, script file, or executable program.
    ```

    -   Solution:

    -   Ensure the Executable Is in PATH: Verify that the path to CustomSftpTool\publish is added to your PATH.
    -   Use Quotation Marks for Multi-word Names: If your profile name contains spaces, enclose it in quotes.
    -   Restart PowerShell: Close and reopen PowerShell to apply PATH changes.

    2. Profile Not Found

    -   Symptom:

    ```powershell
    Error: Profile 'Profile Name' not found.
    ```

    Cause:

    -   Attempting to access a multi-word profile without proper quoting or concatenation.

    -   Solution:

    -   Use Quotation Marks:

    ```powershell
    customSFTP --edit-profile "Profile Name"
    ```

    3. Compiler Warnings

    Symptom:

    -   Warnings related to null reference types during build or publish.

    Solution:

    -   Ensure Nullable Reference Types Are Handled: Initialize all non-nullable properties.
        Review and Update Code: Follow the updated code snippets provided to eliminate these warnings.

    4. Deployment Fails

    Symptom:

    -   Deployment process fails at a specific step.

    -   Solution:

        -   Check Logs: Review deployment.log for detailed error messages.
        -   Verify SSH Credentials: Ensure SSH host, username, and private key path are correct.
        -   Check Remote Service: Verify that the service name is correct.
        -   Validate Paths: Ensure local and remote directories exist and are accessible.

    -   Permissions Issues

    -   Symptom:

        -   Errors related to file access or SSH permissions.

    -   Solution:

        -   Run as Administrator: On Windows, try running PowerShell as an administrator.
        -   SSH Key Permissions: Ensure that your SSH private key has the correct permissions.
        -   Remote Server Permissions: Verify that the SSH user has the necessary permissions to manage services and transfer files.

    5. Permissions Issues

    -   Symptom:

        -   Errors related to file access or SSH permissions.

    -   Solution:

        -   Run as Administrator: On Windows, try running PowerShell as an administrator.
        -   SSH Key Permissions: Ensure that your SSH private key has the correct permissions.
        -   Remote Server Permissions: Verify that the SSH user has the necessary permissions to manage services and transfer files.
