# SendMSG – Multi-Client Encrypted Chat & Cyber Defense Project

SendMSG is a robust, multi-client chat system developed as a 5-unit Cyber Security project. It features a secure communication architecture utilizing hybrid encryption (RSA + AES) to protect users from local network attacks. The project also includes a dedicated "Hacker" module to demonstrate the mechanics of ARP Spoofing and Man-in-the-Middle (MITM) attacks.

## 🚀 How To Run

> [!IMPORTANT]
> This project is designed to run on **Windows OS**.

### 1. Server and Client
To start the chat system, navigate to the project build folder and run the following executables:
* **Server:** Run `SendMsgServer.exe`. On the first launch, ensure your SQL Server is active!
* **Client:** Run `SendMsgClient.exe`. You can open multiple instances of the client to simulate different users.

### 2. Hacker Module
To run the Man-in-the-Middle (MITM) and ARP Spoofing demonstration tool, ensure you have Python 3 installed with the `scapy` library, then run:
```bash
cd Hacker
python3 hacker.py
```

## ✨ Features
* **Multi-Client Communication:** Supports multiple simultaneous connections using asynchronous multithreading.
* **Secure Messaging:** Full end-to-end encryption for all text-based messages.
* **File Transfer:** Securely upload and download files within chat rooms with integrated integrity checks.
* **Group Management:** Create private rooms and manage participants with Admin-only controls.
* **Integrated Chess:** Play real-time chess games against other clients directly within the application.
* **Chat Persistence:** Microsoft SQL Server backend to store user credentials, message history, and room data.

## 🏗 System Architecture



The system follows a Client-Server model with a specific focus on thread management to ensure a responsive UI and stable server performance.

### Key Modules:
* **Server (C#):** Manages the central logic, client threads, and SQL database interactions.
* **Client (C#/WPF):** Provides a rich graphical user interface for messaging and gaming.
* **EncryptedSocket:** A custom wrapper class that handles the RSA/AES encryption handshake and transparent data encryption for all socket traffic.

## 🔐 Security Implementation



The core of SendMSG's defense is its hybrid encryption protocol designed to prevent Man-in-the-Middle (MITM) data theft:
1. **Asymmetric Handshake:** Upon connection, the client and server exchange RSA public keys.
2. **Symmetric Key Exchange:** A unique session AES key is generated and transmitted encrypted via RSA.
3. **Continuous Encryption:** All subsequent traffic (messages, file metadata, chess moves) is encrypted using AES-256 for high-speed security.

## 🕵️ Hacker Module (Educational)
The project includes a Python-based tool using the **Scapy** library to demonstrate network vulnerabilities:
* **ARP Poisoning:** Floods the local network with forged ARP responses to redirect traffic through the hacker's machine.
* **MITM Sniffer:** Intercepts, decrypts (if security is disabled), and displays traffic in real-time.
* **Packet Forwarding:** Uses IP forwarding logic to ensure the victim's connection isn't dropped while being monitored.

## 🛠 Tech Stack
* **Backend:** C# (.NET Framework)
* **Frontend:** WPF (XAML)
* **Database:** Microsoft SQL Server
* **Cyber Tooling:** Python 3.x, Scapy
* **Protocols:** TCP/IP, ARP, RSA, AES

## 📊 Database Schema



The SQL database consists of four primary tables to manage the application state:
* **User:** Stores unique IDs, usernames, and hashed passwords.
* **Chat:** Tracks chat room names and their respective owners/creators.
* **Message:** Logs all messages with sender IDs, room IDs, and timestamps.
* **UserChat:** A junction table mapping users to the specific groups they have joined.

---

## 👨‍💻 Author
**Liel Abraham**
*Cyber Security 5-Unit Project | 2025*
*Mentor: Alex Gershberg*
