const fs = require('fs');
const path = require('path');

const version = process.argv[2].replace('v', '');

function getReleaseNotes(owner, repo, version) {
    const apiUrl = `https://api.github.com/repos/${owner}/${repo}/releases`;

    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open('GET', apiUrl, true);

        xhr.onload = () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                const releases = JSON.parse(xhr.responseText);

                const release = releases.find(r => r.tag_name === version);

                if (release) {
                    resolve(release.body);
                }
            } else {
                reject(`Failed to fetch releases. Status code: ${xhr.status}`);
            }
        };

        xhr.onerror = () => {
            reject('Error making the request.');
        };

        xhr.send();
    });
}

async function updateVersionInfo(version) {
    const jsonFilePath = path.join('docs', 'version.json');

    const jsonData = fs.readFileSync(jsonFilePath, 'utf8');
    const jsonContent = JSON.parse(jsonData.replace(/\n/g, '').replace(/\r/g, ''));

    const changelogMarkdown = await getReleaseNotes('0Miles', 'soku-launcher', version);

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
}

updateVersionInfo(version);