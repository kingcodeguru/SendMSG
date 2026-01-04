from colorama import Fore
import os
import sys
import time
from subprocess import Popen, PIPE, CREATE_NEW_CONSOLE

messages = 'This is Console1', 'This is Console2'


# open new consoles
def new_python_run(path, parameters):
    execute = [sys.executable, path]
    for parameter in parameters:
        execute.append(parameter)
    return Popen(execute,
                 stdin=PIPE, bufsize=1, universal_newlines=True,
                 creationflags=CREATE_NEW_CONSOLE)


def active_process(p: Popen):
    return p is not None and p.poll() is None


def present_options(options: list[str], processes: list[Popen]):
    print('The action you want to do:')
    x = len(str(len(options)))
    for i in range(len(options)):
        print(f'{Fore.LIGHTGREEN_EX if active_process(processes[i]) else Fore.LIGHTRED_EX}[{'+' if active_process(processes[i]) else '-'}] {Fore.WHITE}{str(i + 1).rjust(x)}) {options[i]}')
    print('Your choice:', end=' ')


def get_start_ip(ip, subnet_mask) -> str:
    subnet_mask_cast = subnet_mask.split('.')
    ip_cast = ip.split('.')
    ret = ''
    for i in range(4):
        if int(subnet_mask_cast[i]) == 255:
            ret += f'{ip_cast[i]}.'
    return ret


def main():
    os.system('cls')
    os.system('mode 100, 30')
    processes_list: list[Popen] = [None] * 5
    paths: list[str] = [r'poison.py', r'getAllIP.py', r'keepNetWork.py', r'show_messages.py']
    options_names = ['ARP spoof', 'Receive all ip in the local network', 'Forward packets', 'Present messages', 'Exit']
    subnet_mask = input('Enter your  subnet mask: ')
    if subnet_mask == 'd':
        subnet_mask = '255.255.255.0'
    my_ip = input('Enter your   IP address: ')
    default = get_start_ip(my_ip, subnet_mask)
    client_ip = input(f'Enter victim IP address: {default}')
    client_ip = default + client_ip
    server_ip = input(f'Enter server IP address: {default}')
    server_ip = default + server_ip
    while True:
        os.system('cls')
        print(f'{Fore.CYAN}My IP     = {my_ip}\nClient IP = {client_ip}\nServer IP = {server_ip}{Fore.WHITE}')
        present_options(options_names, processes_list)
        inp = int(input())
        if inp == 5:
            os.system('cls')
            break
        inp -= 1
        if 0 <= inp <= 3:
            if active_process(processes_list[inp]):
                processes_list[inp].terminate()
            else:
                if inp == 0:
                    processes_list[inp] = new_python_run(paths[inp], [client_ip, server_ip])
                elif inp == 1:
                    processes_list[inp] = new_python_run(paths[inp], [my_ip, subnet_mask])
                elif inp == 2:
                    processes_list[inp] = new_python_run(paths[inp], [client_ip, server_ip, my_ip])
                else:
                    processes_list[inp] = new_python_run(paths[inp], [client_ip, server_ip, my_ip])
        else:
            print('Invalid')
    for proc in processes_list:
        try:
            proc.terminate()
        except AttributeError:
            pass


if __name__ == '__main__':
    main()
