import os
import subprocess
import sys
import threading
import time


ip = ''
subnet_mask = ''


def check_connection(cmd, ip, ips):
    cmd += ' ' + ip
    ret = subprocess.getoutput(cmd)
    ret = ret[ret.find('Reply from'):]
    ret = ret[:ret.find('\n')]
    if 'bytes' in ret[ret.find('Reply from'):]:
        ips.append(ip)



def get_all_ip() -> list[str]:
    global ip
    global subnet_mask
    possible = '{}.{}.{}.{}'
    subnet_mask_cast = subnet_mask.split('.')
    ip_cast = ip.split('.')
    default_ip = [int(ip_cast[i]) & int(subnet_mask_cast[i]) for i in range(4)]
    cmd = 'ping -n 1'
    thread_list = []
    ip_list = []
    for a in range(default_ip[0], default_ip[0] + 256 - int(subnet_mask_cast[0])):
        for b in range(default_ip[1], default_ip[1] + 256 - int(subnet_mask_cast[1])):
            for c in range(default_ip[2], default_ip[2] + 256 - int(subnet_mask_cast[2])):
                for d in range(default_ip[3], default_ip[3] + 256 - int(subnet_mask_cast[3])):
                    t = threading.Thread(target=check_connection, args=(cmd, possible.format(a, b, c, d), ip_list))
                    thread_list.append(t)
                    t.start()
    for x in range(255):
        thread_list[x].join()
    ip_list.sort()
    return ip_list


def all_connected_ips():
    print('looking for ips in the local network...')
    try:
        ip_list = get_all_ip()
    except:
        time.sleep(2)
    n = len(ip_list)
    for i in range(len(ip_list)):
        ip = ip_list[i]
        print(f'{str(i + 1).zfill(len(str(n)))}) {ip}')
    print(f'there are {n} online users in your local network')
    while True:
        time.sleep(10)


if __name__ == '__main__':
    os.system('mode 50, 30')
    sys.argv.pop(0)
    args = sys.argv
    if len(args) != 2:
        print('Invalid number of arguments')
        time.sleep(2)
        exit(1)
    ip = args[0]
    subnet_mask = args[1]
    all_connected_ips()
