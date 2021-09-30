using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System.Text;
using System.Text.Json;

var globalEndpoint = "global.azure-devices-provisioning.net";
var idScope = "0ne003D5FD9";
var registrationId = "";
var primaryKey = "=";
var secondaryKey = "+Thd8ISV+/qVdeae+GnDwYiIo=";
var securityProvider = new SecurityProviderSymmetricKey(registrationId, primaryKey, secondaryKey);

var dpsClient = ProvisioningDeviceClient.Create(
        globalEndpoint, 
        idScope,
        securityProvider,
        new ProvisioningTransportHandlerMqtt());

var registrationResult = await dpsClient.RegisterAsync();

var deviceAuthentication = new DeviceAuthenticationWithRegistrySymmetricKey(
                registrationResult.DeviceId,
                securityProvider.GetPrimaryKey());

using var deviceClient = DeviceClient.Create(registrationResult.AssignedHub, deviceAuthentication, TransportType.Mqtt);

var payload = new Models.Solar_Panel_v1_1_7k6();

while (true)
{
    var payload_json = JsonSerializer.Serialize(payload);
    var payload_bytes = Encoding.UTF8.GetBytes(payload_json);
    var message = new Message(payload_bytes);

    await deviceClient.SendEventAsync(message);

    payload.Update();
    Console.WriteLine(payload_json);
    await Task.Delay(500);
}

Console.ReadLine();

