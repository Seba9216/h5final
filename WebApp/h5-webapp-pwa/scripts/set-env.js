// scripts/set-env.js
const { readFileSync, writeFileSync } = require('fs');
const dotenv = require('dotenv');
const { resolve } = require('path');

dotenv.config({ path: resolve(__dirname, '../../../.env') });


const envContent = `
export const environment = {
  production: false,
  ApiUrl: '${process.env.ApiUrl}',
  WebSocketUrl: '${process.env.WebSocketUrl}'
};
`;

writeFileSync('./src/environments/environment.ts', envContent);
console.log('environment.ts generated!');