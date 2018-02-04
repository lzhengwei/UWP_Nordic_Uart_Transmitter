# UWP Nordic Uart Transmitter

This is a Windows10 UWP C# project.

Goal of this project is connect to Nordic IC nrf51822(MDBT40) through Ble(Bluetooth Low Energy).

This program has basic uart function include **list and connect to ble device**、**list Ble device's UUID**、**send and receive data***

Divide into two project

| Project  | function |
| ---------- | ----------- |
| BLE_GATT_list  | List all Ble divice GATT UUID |
| BLE_uart_connector | As a connector to Ble divice, Use Nordic Uart UUID to sned/receive data form device |

# Artitecture 

Nrf51822(MDBT40) connect to android or windows10 UWP application send raw data through Bluetooth Low Energy(BLE).

(I have another project for Nrf51822 program which develope on keilC5 name as Nrf51822 with LSM9DS)

LSM9DS、LIS3DH and Micro SD card connect to Nrf51822 through SPI protcol.

![artitecture](https://github.com/lzhengwei/UWP_Nordic_Uart_Transmitter/blob/master/Structure.jpg)
