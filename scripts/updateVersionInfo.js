const fs = require('fs');
const path = require('path');

const version = process.argv[2].replace('v', '');

const jsonFilePath = path.join('docs', 'version.json');

const jsonData = fs.readFileSync(jsonFilePath, 'utf8');
const jsonContent = JSON.parse(jsonData.replace(/\n/g, '').replace(/\r/g, ''));

const changelogMarkdown = fs.readFileSync('changelog.md', 'utf8');

jsonContent[0].Version = version
jsonContent[0].DownloadUrl = `https://github.com/0Miles/soku-launcher/releases/download/v${version}/SokuLauncher.zip`
jsonContent[0].DownloadLinks = [
    {
        Type: 'Github',
        Url: `https://github.com/0Miles/soku-launcher/releases/download/v${version}/SokuLauncher.zip`
    }
]
jsonContent[0].Notes = changelogMarkdown;

const updatedJsonData = JSON.stringify(jsonContent, null, 4);

fs.writeFileSync(jsonFilePath, updatedJsonData, 'utf8');