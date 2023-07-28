# Wireguard Multipath
This repository houses source code to the Multipath application used by Ping Tico (Costa-rican Gaming VPN service) to connect users using Wireguard tunnels through multiple servers.

## How it works
This application runs on both ends of a Wireguard tunnel (Server and Client).
### Client-side:
  - Receives packets locally from Wireguard (Usually 127.0.0.1:<'Multipath Listen Port'>)
  - Duplicates packets received and sends them to every route (<IP>:<Port>)
  - Any packets received back from these routes are returned to Wireguard locally
### Server-side
  - Listens for any incoming packets, once a new packet is detected, a connection is established to allow packets to be sent back.
  - Forwards all received packets to the specified <IP>:<Port>, usually locally to the Wireguard server instance.
  - Any packets received back from Wireguard are then forwarded back through each route established by the client side.

**Note:** The application does not de-duplicate packets, it relies on the de-duplication feature that comes built-in on Wireguard. If using this for different purposes, you must implement de-duplication in case the final application does not support it.

### High-level overview
![image](https://github.com/vinivyes/Wireguard-Multipath/assets/32873823/48a0ffac-c6ad-43fb-8c67-7b208d612caa)

You can reach out to me on: viniciusmorais352@gmail.com
