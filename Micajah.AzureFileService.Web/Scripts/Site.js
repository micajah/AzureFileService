function SwitchTheme() {
    var isThemeDark = document.documentElement.classList.contains("theme-dark");

    if (isThemeDark) {
        document.documentElement.classList.remove("theme-dark");
    }
    else {
        document.documentElement.classList.add("theme-dark");
    }

    Opentip.reset();
}

function BeginRequestHandler() {
    document.body.style.cursor = 'wait';
}

function EndRequestHandler() {
    document.body.style.cursor = 'default';
}

if (typeof (Sys) !== "undefined") {
    if (Sys.WebForms) {
        if (Sys.WebForms.PageRequestManager)
            var instance = Sys.WebForms.PageRequestManager.getInstance();
        instance.add_beginRequest(BeginRequestHandler)
        instance.add_endRequest(EndRequestHandler);
    }
}