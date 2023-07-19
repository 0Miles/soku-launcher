const fs = require('fs');
const path = require('path');

const version = process.argv[2].replace('v', '');

const jsonFilePath = path.join('docs', 'version.json');

const jsonData = fs.readFileSync(jsonFilePath, 'utf8');
const jsonContent = JSON.parse(jsonData.replace(/\n/g, '').replace(/\r/g, ''));

jsonContent[0].Version = version
jsonContent[0].DownloadUrl = `https://github.com/0Miles/soku-launcher/releases/download/v${version}/SokuLauncher.zip`

const updatedJsonData = JSON.stringify(jsonContent, null, 4);

fs.writeFileSync(jsonFilePath, updatedJsonData, 'utf8');