import { dotnet } from './_framework/dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = dotnetRuntime.getConfig();

const runPromise = dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);

// Hide splash overlay once Avalonia has had time to render its first frame
requestAnimationFrame(() => requestAnimationFrame(() => {
    const splash = document.querySelector('.avalonia-splash');
    if (splash) splash.classList.add('loaded');
}));

await runPromise;
