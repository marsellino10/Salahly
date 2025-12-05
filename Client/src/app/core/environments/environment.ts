interface FireworksEnvironmentConfig {
  baseUrl: string;
  apiKey: string;
  model: string;
}

interface SalahlyEnvironment {
  baseApi: string;
  production: boolean;
  fireworks: FireworksEnvironmentConfig;
}

export const environment: SalahlyEnvironment = {
  baseApi: "http://salahly.runasp.net/api/",
  production: false,
  fireworks: {
    baseUrl: 'https://api.fireworks.ai/inference/v1',
    apiKey: 'fw_3ZbZueFQEaBhpmDWaWdLCNUc',
    model: 'accounts/fireworks/models/llama-v3p1-8b-instruct',
  },
};
