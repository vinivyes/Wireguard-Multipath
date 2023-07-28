# Wireguard Multipath
This repository houses source code to the Multipath application used by Ping Tico (Costa-rican Gaming VPN service) to connect users using Wireguard tunnels through multiple servers.

## How it works
This application runs on both ends of a Wireguard tunnel (Server and Client), on the client-side, Wireguard sends packets locally to the application, the application creates a copy of the packet and sends it over route in a list. Usually, these routes include a direct connection to the server and additionally, servers that act as relays, forwarding packets to the server running Wireguard. On the server-side, another instance of this application is running to funnel all packets to Wireguard, register any incoming connections to return packets to them, after a connection is established, any packets generated on the server are forwarded back through every single route.

### High-level overview
![image](https://github.com/vinivyes/Wireguard-Multipath/assets/32873823/48a0ffac-c6ad-43fb-8c67-7b208d612caa)

You can reach out to me on: viniciusmorais352@gmail.com
