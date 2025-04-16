import { existsSync, readdirSync, readFileSync, writeFileSync } from 'fs';
import { execSync } from 'child_process';
import { createHash } from 'crypto';
import process from 'process';

const match = readFileSync('SakuraLibrary/Consts.cs', 'utf8').match(/Version = "(.+)";/);
if (!match) throw new Error("Version not found in SakuraLibrary\\Consts.cs");
const version = match[1];

let signtool = readdirSync('C:/Program Files (x86)/Windows Kits/10/bin')
    .filter(v => v.startsWith('10.0.'))
    .sort((a, b) => Number(b.split('.')[2]) - Number(a.split('.')[2]))[0];
if(!signtool) throw new Error('Windows SDK not found');

signtool = `C:/Program Files (x86)/Windows Kits/10/bin/${signtool}/x64/signtool.exe`;
if (!existsSync(signtool)) throw new Error("signtool.exe not found");

console.info(`Using ${signtool}`);
signtool = `"${signtool}"`;

const certHash = process.env.SAKURA_SIGN_CERT;
if (!certHash) throw new Error("Environment variable 'SAKURA_SIGN_CERT' is not set");

execSync(`${signtool} sign /sha1 ${certHash} /td SHA1 /tr http://timestamp.digicert.com /fd SHA1 /as "bin/SakuraLauncher.exe"`, { stdio: 'inherit' });
execSync(`${signtool} sign /sha1 ${certHash} /td SHA256 /tr http://timestamp.digicert.com /fd SHA256 /as "bin/SakuraLauncher.exe"`, { stdio: 'inherit' });

const pkgHash = createHash('sha256').update(readFileSync('bin/SakuraLauncher.exe')).digest('hex').toUpperCase();

writeFileSync('bin/SakuraLauncher.ps1', readFileSync('install_template.ps1', 'utf8')
    .replace('%INSTALLER_MD5%', pkgHash)
    .replace('%INSTALLER_URL%', `https://nya.globalslb.net/natfrp/client/launcher-windows/${version}/SakuraLauncher.exe`),
    { encoding: 'utf8' }
);

execSync(`${signtool} sign /sha1 ${certHash} /td SHA256 /tr http://timestamp.digicert.com /fd SHA256 "bin/SakuraLauncher.ps1"`, { stdio: 'inherit' });
