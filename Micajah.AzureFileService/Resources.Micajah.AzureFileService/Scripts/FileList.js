Opentip.styles.mafs = {
    extends: "glass",
    className: "mafs",
    group: "mafs",
    showOn: "mouseover",
    hideOn: "mouseout",
    hideTrigger: "tip",
    target: true,
    targetJoint: "center center",
    tipJoint: "left top",
    removeElementsOnHide: true,
    borderRadius: 12,
    shadowColor: "#0000002C",
    background: "#FFFFFF",
    borderColor: "#CCCCCC"
}

Opentip.styles.mafsDark = {
    extends: "mafs",
    background: "#062D4C",
    borderColor: "#36556E"
}

Opentip.resetDefaultStyle = function () {
    var isThemeDark = document.documentElement.classList.contains("theme-dark");

    Opentip.defaultStyle = isThemeDark ? "mafsDark" : "mafs";
}

Opentip.reset = function () {
    for (var i = 0; i < Opentip.tips.length; i++) {
        var tip = Opentip.tips[i];
        tip.options.hideEffectDuration = 0;
        tip.options.hideDelay = 0;
        tip.deactivate();
    }

    Opentip.resetDefaultStyle();

    Opentip.lastId = 0;

    firstAdapter = true;
    Opentip.adapters = {};
    Opentip.adapter = null;
    Opentip.addAdapter(new Adapter);
}

Opentip.lastZIndex = 3001;

Opentip.resetDefaultStyle();