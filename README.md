# MailClient.App

A cross-platform .NET console application for sending and receiving emails. Servers are fully configurable via a simple interactive prompt, and all email communication is powered by the **MailClient** library, which implements custom SSL/TLS IMAP and SMTP clients.

## Features

- **Add Server**: Register new IMAP/SMTP server configurations (host, port, credentials, display name).
- **Select Server**: Choose among saved servers to perform operations.
- **Delete Server**: Remove a saved server configuration.
- **Fetch Inbox**: Retrieve new messages from the selected IMAP mailbox, store them locally, and display them.
- **Send Email**: Compose and send a message via SMTP using the selected server’s credentials.
- **Exit**: Cleanly close connections and quit the application.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Access to IMAP (port 993) and/or SMTP (port 465/587) servers with valid credentials

### Running the App

1. Clone the repo:
   ```bash
   git clone https://github.com/yourusername/mailclient.app.git
   cd mailclient.app
   ```
2. Build and run:
   ```bash
   dotnet run --project MailClient.App
   ```
3. You’ll see a menu with numeric options:
   ```text
   1. Add Server
   2. Select Server
   3. Delete Server
   4. Fetch Inbox
   5. Send Email
   6. Exit
   ```
4. Enter the number of the operation you wish to perform.

## Menu Operations

| Option | Command       | Description                                                                                  |
| ------ | ------------- | -------------------------------------------------------------------------------------------- |
| **1**  | Add Server    | Prompt for IMAP/SMTP host, port, username, password, and display name; saves configuration. |
| **2**  | Select Server | Choose one of the previously saved servers to act on.                                        |
| **3**  | Delete Server | Remove a saved server entry by selecting it from the list.                                   |
| **4**  | Fetch Inbox   | Connects via IMAP, fetches new messages (by UID), stores `.eml` files locally, and displays summary. |
| **5**  | Send Email    | Prompts for recipient(s), subject, body, then connects via SMTP to send the email.           |
| **6**  | Exit          | Sends `QUIT`/`LOGOUT` to servers, disposes resources, and exits.                              |

## Architecture

- **MailClient Library**:
  - `ImapClient`: Custom IMAP-over-SSL/TLS implementation for fetching raw RFC-822 messages.
  - `SmtpClient`: Custom SMTP-over-SSL/TLS implementation (AUTH PLAIN) for sending emails.

- **App Infrastructure**:
  - **Prompts**: Interactive console prompts for user input with validation and cancellation support.
  - **Persistence**: Stores server configurations (EF Core in local SQLite) and downloaded emails (`.eml` files under `%APPDATA%`).
  - **MediatR Handlers**: Commands and queries wired via MediatR for clean separation of concerns.
  - **Parsing**: Parses raw email text into header/body via `EmailMessageParser`.

## Configuration

Server configurations are persisted automatically. Downloaded emails are saved to:
```
%APPDATA%\MailClient\<ServerAddress>\Emails\<UID>.eml
```

## Logging

Uses **Serilog** to log all client-server commands and responses to the console for easy debugging.

## License

MIT License. See [LICENSE](LICENSE) for details.
