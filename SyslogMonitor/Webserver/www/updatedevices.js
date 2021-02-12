console.log("script");
var HttpClient = function() {

    this.get = function(aUrl, aCallback) {
        var anHttpRequest = new XMLHttpRequest();

        anHttpRequest.onreadystatechange = function() {
            if (anHttpRequest.readyState == 4 && anHttpRequest.status == 200)
                aCallback(anHttpRequest.responseText);
        };
        anHttpRequest.open("GET", aUrl, true);
        anHttpRequest.setRequestHeader("API_KEY", "dit-is-een-api-key");
        anHttpRequest.send(null);
    };
}


function getDeviceData() {
    var client = new HttpClient();
    client.get('/api/devices', function(response) {
        var devices = JSON.parse(response);
        let parsedDeviceList = [];
        for (var device in devices) {
            let id = devices[device].DeviceId;
            let deviceMac = devices[device].DeviceMac;
            let deviceIp = devices[device].DeviceIp;
            let deviceConnected = devices[device].DeviceConnected;
            let deviceName = devices[device].DeviceName;
            let deviceLastUpdated = new Date(devices[device].LastUpdate);
            let connectionHost = devices[device].Host;
            parsedDeviceList.push({
                DeviceName: deviceName,
                DeviceMac: deviceMac,
                DeviceIp: deviceIp,
                DeviceLastUpdated: deviceLastUpdated.toLocaleString("nl-NL"),
                DeviceConnected: deviceConnected,
                Host: connectionHost,
                Id: id,
            });
        }
        sortDeviceList(parsedDeviceList);
        updateTable(parsedDeviceList);
    });
}

function updateTable(data) {
    let table = document.getElementById('deviceTable');
    table.innerHTML = "";

    for (let element of data) {
        let row = table.insertRow();
        if (element["DeviceConnected"] === true) {
            row.classList = "connected"
        } else if (element["DeviceConnected"] === false) {
            row.classList = "disconnected"
        }
        for (key in element) {
            //loop through the element list until you find the id column, then add an edit button.
            if (key !== "Id") {
                let cell = row.insertCell();
                let text = document.createTextNode(element[key]);

                if (element[key] === null) {
                    cell.style.backgroundColor = '#ea71716b';
                    let cursive = document.createElement('i');
                    cursive.appendChild(text);
                    cell.appendChild(cursive);
                } else {
                    cell.appendChild(text);
                }
            } else {
                let cell = row.insertCell();
                let aButton = document.createElement("button");
                aButton.innerText = "Edit";
                aButton.dataset.deviceid = element[key];
                aButton.onclick = function(e) {
                    console.log(e);
                    console.log(this.dataset.deviceid);
                };
                cell.appendChild(aButton);
            }
        }
    }
}

function sortDeviceList(data) {
    data.sort((a, b) => {
        if (a["DeviceIp"] === null && b["DeviceIp"] === null) {
            return -1;
        } else if (a["DeviceIp"] === null && b["DeviceIp"] !== null) {
            return 1;
        }
        const num1 = Number(a["DeviceIp"].split(".").map((num) => (`000${num}`).slice(-3)).join(""));
        if (b["DeviceIp"] === null) {
            return -1;
        }
        const num2 = Number(b["DeviceIp"].split(".").map((num) => (`000${num}`).slice(-3)).join(""));
        return num1 - num2;
    });
}