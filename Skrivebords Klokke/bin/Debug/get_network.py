import psutil
import time
def get_Tot_R_Bytes():
    return (psutil.net_io_counters(pernic=True)['Ethernet'])[1]

old_Tot_Bytes = get_Tot_R_Bytes()
test = str(old_Tot_Bytes)
with open('test.txt', 'w') as f:
    f.write(test)


def printMB_s():
    global old_Tot_Bytes
    cur_tot_bytes = get_Tot_R_Bytes()
    bytes_Diff = cur_tot_bytes - old_Tot_Bytes
    print bytes_Diff
    old_Tot_Bytes = cur_tot_bytes
    down_speed = int(bytes_Diff)
    down_speed = str(down_speed)
    print down_speed + "KB/s"

    global my_file
    with open('download_speed.txt', 'w') as f:
        f.write(down_speed)

    time.sleep(1)

while True:
    printMB_s()