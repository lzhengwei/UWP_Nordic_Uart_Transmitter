using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.Security.Cryptography;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Threading.Tasks;
// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace App1
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BluetoothLEAdvertisementPublisher publisher = new BluetoothLEAdvertisementPublisher();
        BluetoothLEAdvertisementWatcher bleWatvher = new BluetoothLEAdvertisementWatcher();
        BluetoothLEDevice bleDevice;
        DeviceWatcher devWatcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelector(), null);
        List<String> address = new List<string>(50);
        String addresslist = "";

        IReadOnlyList<GattDeviceService> myGATTservicelist;

        String UUID_SERV = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
        String UUID_WRITE = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
        String UUID_READ = "6e400003-b5a3-f393-e0a9-e50e24dcca9e"; // 0: disable,1: enable
        String receive_Text = "";
        GattDeviceService UARTservice;
        GattCharacteristic WRITEcharacteristic, READcharacteristic;

        private List<BluetoothDevice> listOfBledevice = new List<BluetoothDevice>();
        private List<BLEGattUUID> listOfBleUUID = new List<BLEGattUUID>();

        private List<String> listOfBledeviceName = new List<String>();
        private List<String> listofUUID = new List<String>();

        int receivetimes = 0;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            listOfBledeviceName.Clear();
            //advertisment_init();
            listOfBledevice.Clear();
            Bloos = new ObservableCollection<BluetoothDevice>();
            startScanner();
        }
        private void advertisment_init()
        {
            // Query for extra properties you want returned
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher =
                        DeviceInformation.CreateWatcher(
                                BluetoothLEDevice.GetDeviceSelector(),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);
            /*    deviceWatcher.Added += DeviceWatcher_Added;
                deviceWatcher.Updated += DeviceWatcher_Updated;
                deviceWatcher.Removed += DeviceWatcher_Removed;

                // EnumerationCompleted and Stopped are optional to implement.
                deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped += DeviceWatcher_Stopped;*/

            // Start the watcher.
            deviceWatcher.Start();
        }
  
        public ObservableCollection<BluetoothDevice> Bloos
        {
            get;
            set;
        }
        public void startScanner()
        {
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher =
                        DeviceInformation.CreateWatcher(
                                BluetoothLEDevice.GetDeviceSelector(),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);
            bleWatvher.ScanningMode = BluetoothLEScanningMode.Active;
            
            bleWatvher.Received += BleWatvher_Received;
            bleWatvher.Stopped += BleWatvher_Stopped;

            deviceWatcher.Stopped += DevWatcher_Stopped;
            deviceWatcher.Added += DevWatcher_Added;

            bleWatvher.Start();
            //deviceWatcher.Start();
        }
        private void DevWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            //textbox.Text += args.Name+"\n";
            addresslist += args.Name + " " + args.EnclosureLocation + "\n";
            // ConnectDevice(args);
        }
        private void DevWatcher_Stopped(DeviceWatcher sender, object args)
        {
            devWatcher.Start();
        }
        private void BleWatvher_Stopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            bleWatvher.Start();
        }
        List<String> listOfdata = new List<String>();
        private async void BleWatvher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            //textbox.Text = "gg";
            
            //addresslist += args.Advertisement.LocalName + " " + args.BluetoothAddress;
            //var bledevice = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
            if (args.Advertisement.LocalName != "" && !listOfBledeviceName.Contains(args.BluetoothAddress + ""))
            {
                address.Add(args.Advertisement.LocalName);
                
                listOfBledeviceName.Add(args.BluetoothAddress+"");
                listOfBledevice.Add(new BluetoothDevice { Address = args.BluetoothAddress, Name = args.Advertisement.LocalName,SignalStrength= args.RawSignalStrengthInDBm });
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    listview1.ItemsSource = null;
                    listview1.ItemsSource = listOfBledevice;
                });

            }


        }

        byte[] compareinput=new byte[18];
        void Uart_Receive(GattCharacteristic sender,
                                    GattValueChangedEventArgs args)
        {
           
        }
        int listviewSelectindex = -1;
        private void listview1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listviewSelectindex = listview1.SelectedIndex;
            show_GATT_UUID(listOfBledevice[listviewSelectindex].Address);
        }
        private void listview1_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (listviewSelectindex == listview1.SelectedIndex && listview1.SelectedIndex != -1)
            {
                show_GATT_UUID(listOfBledevice[listviewSelectindex].Address);
            }
        }

        int listviewSelectindex_uuid = -1;
        private void listview_uuid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(listview_uuid.SelectedIndex!=-1)
            {
                 listviewSelectindex_uuid = listview_uuid.SelectedIndex;
                 show_Service_UUID(listOfBleUUID[listviewSelectindex_uuid].UUID);
            }
           
        }

        private void listview_uuid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (listviewSelectindex_uuid == listview_uuid.SelectedIndex && listview_uuid.SelectedIndex != -1)
            {
                show_Service_UUID(listOfBleUUID[listviewSelectindex_uuid].UUID);
            }
        }

        private async void show_GATT_UUID(ulong address)
        {
            bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
            IReadOnlyList<GattDeviceService> GATTservicelist= bleDevice.GattServices;
            listOfBleUUID.Clear();
            for (int i = 0; i < GATTservicelist.Count; i++)
            {
                listOfBleUUID.Add(new BLEGattUUID { UUID=GATTservicelist[i].Uuid + "" });
            }
            listview_uuid.ItemsSource = null;
            listview_uuid.ItemsSource = listOfBleUUID;


        }

        private async void show_Service_UUID(string address)
        {
            try
            {
                var characteristic = bleDevice.GetGattService(Guid.Parse(address)).GetAllCharacteristics();
                listOfBleUUID.Clear();
                for (int i = 0; i < characteristic.Count; i++)
                {
                    listOfBleUUID.Add(new BLEGattUUID { UUID = characteristic[i].Uuid + "" ,Properties= characteristic[i].CharacteristicProperties.ToString()});
                }
                listview_uuid.ItemsSource = null;
                listview_uuid.ItemsSource = listOfBleUUID;
            }
            catch (Exception xx)
            {

            }


            
        }


    }
    public class BluetoothDevice
    {
        public ulong Address { get; set; }
        public string Name{ get; set; }
        public short SignalStrength { get; set; }
    }
    public class BLEGattUUID
    {
        public string Properties { get; set; }
        public string UUID { get; set; }
    }
}
