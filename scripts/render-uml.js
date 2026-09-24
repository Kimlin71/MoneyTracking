const fs = require('fs');
const path = require('path');
const zlib = require('zlib');
const https = require('https');

const PLANTUML_BASE64 = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_';

function encode6bit(b) {
    if (b < 10) return String.fromCharCode(48 + b);
    b -= 10;
    if (b < 26) return String.fromCharCode(65 + b);
    b -= 26;
    if (b < 26) return String.fromCharCode(97 + b);
    b -= 26;
    if (b === 0) return '-';
    if (b === 1) return '_';
    return '?';
}

function append3bytes(b1, b2, b3) {
    const c1 = b1 >> 2;
    const c2 = ((b1 & 0x3) << 4) | (b2 >> 4);
    const c3 = ((b2 & 0xF) << 2) | (b3 >> 6);
    const c4 = b3 & 0x3F;
    return encode6bit(c1) + encode6bit(c2) + encode6bit(c3) + encode6bit(c4);
}

function encodePlantUML(text) {
    const compressed = zlib.deflateRawSync(Buffer.from(text, 'utf8'));
    let result = '';
    for (let i = 0; i < compressed.length; i += 3) {
        if (i + 2 === compressed.length) {
            result += append3bytes(compressed[i], compressed[i + 1], 0);
        } else if (i + 1 === compressed.length) {
            result += append3bytes(compressed[i], 0, 0);
        } else {
            result += append3bytes(compressed[i], compressed[i + 1], compressed[i + 2]);
        }
    }
    return result;
}

function downloadPng(encoded, outputPath) {
    return new Promise((resolve, reject) => {
        const url = `https://www.plantuml.com/plantuml/png/${encoded}`;
        const file = fs.createWriteStream(outputPath);
        https.get(url, (response) => {
            if (response.statusCode !== 200) {
                reject(new Error(`HTTP ${response.statusCode}`));
                return;
            }
            response.pipe(file);
            file.on('finish', () => {
                file.close();
                resolve();
            });
        }).on('error', (err) => {
            fs.unlink(outputPath, () => {});
            reject(err);
        });
    });
}

async function main() {
    const docsDir = path.join(__dirname, '..', 'docs');
    const pumlFiles = [
        'uml-class-diagram.puml',
        'uml-use-case-diagram.puml',
        'uml-sequence-add-item.puml',
        'uml-sequence-sort-filter-search.puml',
        'uml-architecture-diagram.puml'
    ];

    for (const file of pumlFiles) {
        const pumlPath = path.join(docsDir, file);
        const pngName = file.replace('.puml', '.png');
        const pngPath = path.join(docsDir, pngName);

        console.log(`Rendering ${file} -> ${pngName} ...`);
        try {
            const pumlText = fs.readFileSync(pumlPath, 'utf8');
            const encoded = encodePlantUML(pumlText);
            await downloadPng(encoded, pngPath);
            const size = fs.statSync(pngPath).size;
            console.log(`  ✓ OK (${Math.round(size / 1024)} KB)`);
        } catch (err) {
            console.log(`  ✗ FAILED: ${err.message}`);
        }
    }

    console.log('\nDone! Check docs/ folder for PNG images.');
}

main().catch(console.error);
