using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectToBluetooth : MonoBehaviour
{
	struct BLE_ScannedItem
	{
		public string RSSI;
		public string Address;
		public string Name;

	}



	public Dropdown dropdown;

	public List<string> possibleNames;

	private List<string> detectedAdresses;

    private float _timeout;
    private float _startScanTimeout = 10f;
    private float _startScanDelay = 0.5f;
    private bool _startScan = true;
    private Dictionary<string, BLE_ScannedItem> _scannedItems;

    // Start is called before the first frame update
    void Start()
    {
		detectedAdresses = new List<string>();
		dropdown.ClearOptions();

		BluetoothLEHardwareInterface.Log("Start");
        _scannedItems = new Dictionary<string, BLE_ScannedItem>();

        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            _timeout = _startScanDelay;
        },
        (error) => {

            BluetoothLEHardwareInterface.Log("Error: " + error);

            if (error.Contains("Bluetooth LE Not Enabled"))
                BluetoothLEHardwareInterface.BluetoothEnable(true);
        });
    }



    // Update is called once per frame
    void Update()
    {
		if (_timeout > 0f)
		{
			_timeout -= Time.deltaTime;
			if (_timeout <= 0f)
			{
				if (_startScan)
				{
					_startScan = false;
					_timeout = _startScanTimeout;

					BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, null, (address, name, rssi, bytes) => {

						BluetoothLEHardwareInterface.Log("item scanned: " + address);
						if (detectedAdresses.Contains(address))
						{
							BluetoothLEHardwareInterface.Log("ALREADY DETECTED");
						}
						else
						{
							BluetoothLEHardwareInterface.Log("item new: " + address);
							BLE_ScannedItem scannedItem = new BLE_ScannedItem();
						

							BluetoothLEHardwareInterface.Log("item set: " + address);

							scannedItem.Address = address;
							scannedItem.Name = name;
							scannedItem.RSSI = rssi.ToString();

							if (possibleNames.Contains(name))
                            {
								_scannedItems[scannedItem.Address] = scannedItem;

								dropdown.ClearOptions();
								List<string> optionNames = new List<string>();
								foreach (KeyValuePair<string, BLE_ScannedItem> s in _scannedItems)
								{
									optionNames.Add(s.Value.Name);
								}
								dropdown.AddOptions(optionNames);
								dropdown.RefreshShownValue();
							}

							detectedAdresses.Add(scannedItem.Name);

							

						}
					}, true);

					BluetoothLEHardwareInterface.Log("THERE ARE THIS MANY ITEMS IN THE DICTIONARY \n\n\n\n\n" + _scannedItems.Count);

					
				}
				else
				{
					BluetoothLEHardwareInterface.StopScan();
					_startScan = true;
					_timeout = _startScanDelay;
				}
			}
		}
	}
}
