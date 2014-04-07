function beginRequestHandler() {
    document.body.style.cursor = 'wait';
}

function endRequestHandler() {
    document.body.style.cursor = 'default';
}

if (typeof (Sys) !== "undefined") {
    if (Sys.WebForms) {
        if (Sys.WebForms.PageRequestManager)
            var instance = Sys.WebForms.PageRequestManager.getInstance();
        instance.add_beginRequest(beginRequestHandler)
        instance.add_endRequest(endRequestHandler);
    }
}