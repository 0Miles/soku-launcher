const fs = require('fs');
const path = require('path');
const https = require('https');

const version = process.argv[2].replace('v', '');
const githubToken = process.argv[3];

function getReleaseNotes(owner, repo, version, githubToken) {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: 'api.github.com',
            path: `/repos/${owner}/${repo}/releases`,
            method: 'GET',
            headers: {
                'User-Agent': 'node.js',
                'Accept': 'application/vnd.github+json',
                'Authorization': `bearer ${githubToken}`,
                'X-GitHub-Api-Version': '2022-11-28'
            }
        };

        const req = https.request(options, (res) => {
            let data = '';

            res.on('data', (chunk) => {
                data += chunk;
            });

            res.on('end', () => {
                const releases = JSON.parse(data);

                const release = releases.find(r => r.tag_name === version);

                if (release) {
                    resolve(release.body);
                } else {
                    reject(`Release not found for version ${version}`);
                }
            });
        });

        req.on('error', (e) => {
            reject(`Error: ${e.message}`);
        });

        req.end();
    });
}

async function updateVersionInfo(version, githubToken) {
    const jsonFilePath = path.join('docs', 'version.json');

    const jsonData = fs.readFileSync(jsonFilePath, 'utf8');
    const jsonContent = JSON.parse(jsonData.replace(/\n/g, '').replace(/\r/g, ''));

    const releaseNotes = await getReleaseNotes('0Miles', 'soku-launcher', 'v' + version, githubToken);

    jsonContent[0].Version = version
    jsonContent[0].DownloadUrl = `https://github.com/0Miles/soku-launcher/releases/download/v${version}/SokuLauncher.zip`
    jsonContent[0].DownloadLinks = [
        {
            Type: 'Github',
            Url: `https://github.com/0Miles/soku-launcher/releases/download/v${version}/SokuLauncher.zip`
        },
        {
            Type: 'Gitee',
            Url: `https://gitee.com/milestw/soku-launcher/releases/download/v${version}/SokuLauncher.zip`
        }
    ]
    jsonContent[0].Notes = releaseNotes;

    const updatedJsonData = JSON.stringify(jsonContent, null, 4);

    fs.writeFileSync(jsonFilePath, updatedJsonData, 'utf8');
}

(async () => {
    await updateVersionInfo(version, githubToken);
})();