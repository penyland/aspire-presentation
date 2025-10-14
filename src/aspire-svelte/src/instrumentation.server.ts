import { NodeSDK } from '@opentelemetry/sdk-node';
import { getNodeAutoInstrumentations } from '@opentelemetry/auto-instrumentations-node';
import { credentials } from '@grpc/grpc-js';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-grpc';
import { createAddHookMessageChannel } from 'import-in-the-middle';
import { register } from 'node:module';
import { resourceFromAttributes } from "@opentelemetry/resources";
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION } from "@opentelemetry/semantic-conventions";
import { diag, DiagConsoleLogger, DiagLogLevel } from '@opentelemetry/api';

diag.setLogger(new DiagConsoleLogger(), DiagLogLevel.INFO);

const { registerOptions } = createAddHookMessageChannel();
register('import-in-the-middle/hook.mjs', import.meta.url, registerOptions);

console.log(`OTEL_EXPORTER_OTLP_ENDPOINT: ${process.env.OTEL_EXPORTER_OTLP_ENDPOINT}`);
const defaultExporterEndpoint = 'http://localhost:4317';
const exporterEndpoint = process.env.OTEL_EXPORTER_OTLP_ENDPOINT || defaultExporterEndpoint;
console.log(`exporterEndpoint: ${exporterEndpoint}`);

const attributes = {
  [ATTR_SERVICE_NAME]: process.env.OTEL_SERVICE_NAME || 'svelte',
  [ATTR_SERVICE_VERSION]: process.env.npm_package_version || 'unknown_version',
  ...parseDelimitedValues(process.env.OTEL_RESOURCE_ATTRIBUTES || '')
}

const isHttps = exporterEndpoint.startsWith('https://');
const collectorOptions = {
  credentials: !isHttps
    ? credentials.createInsecure()
    : credentials.createSsl(),
  url: exporterEndpoint,
};

const sdk = new NodeSDK({
  serviceName: process.env.OTEL_SERVICE_NAME || 'svelte',
  traceExporter: new OTLPTraceExporter(collectorOptions),
  instrumentations: [getNodeAutoInstrumentations()],
  resource: resourceFromAttributes(attributes),
});

sdk.start();

console.log("Instrumentation server initialized");

function parseDelimitedValues(s: string) {
  const headers: string[] = s.split(',').filter(h => h !== ''); // Split by comma
  const o: Record<string, string> = {};

  headers.forEach(header => {
    const [key, value] = header.split('='); // Split by equal sign
    o[key.trim()] = value.trim(); // Add to the object, trimming spaces
  });

  return o;
}