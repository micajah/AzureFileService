var FileUpload;

if (FileUpload == undefined) {
    FileUpload = function (elementId, settings) {
        this.init(elementId, settings);
    };
}

FileUpload.blacklistedBrowsers = [/opera.*Macintosh.*version\/12/i];

FileUpload.isBrowserSupported = function () {
    var capableBrowser, regex, _i, _len, _ref;
    capableBrowser = true;
    if (window.File && window.FileReader && window.FileList && window.Blob && window.FormData && document.querySelector) {
        if (!("classList" in document.createElement("a"))) {
            capableBrowser = false;
        } else {
            _ref = FileUpload.blacklistedBrowsers;
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                regex = _ref[_i];
                if (regex.test(navigator.userAgent)) {
                    capableBrowser = false;
                    continue;
                }
            }
        }
    } else {
        capableBrowser = false;
    }
    return capableBrowser;
};

FileUpload.prototype.init = function (elementId, settings) {
    this.elementId = elementId;
    this.element = document.getElementById(elementId);
    this.element.fileupload = this;

    this.settings = settings;
    this.initSettings();

    var elem;

    if (FileUpload.isBrowserSupported()) {
        elem = document.getElementById(this.elementId + "_IFrame");
        elem.src = this.settings.url;
    }
    else {
        elem = document.getElementById(this.elementId + "_FileFromMyComputer");
    }

    elem.style.display = "";
};

FileUpload.prototype.initSettings = function () {
    this.ensureDefault = function (settingName, defaultValue) {
        this.settings[settingName] = (this.settings[settingName] == undefined) ? defaultValue : this.settings[settingName];
    };

    this.ensureDefault("url", null);

    delete this.ensureDefault;
};

FileUpload.prototype.destroy = function () {
    this.settings = null;

    return delete this.element.fileupload;
};
