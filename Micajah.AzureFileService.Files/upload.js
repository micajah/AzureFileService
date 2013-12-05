Dropzone.autoDiscover = false;
Dropzone.options.fileUpload1 = false;

var urlParams = {},
    match,
    pl = /\+/g,
    search = /([^&=]+)=?([^&]*)/g,
    query = window.location.search.substring(1);

function decode(s) {
    return decodeURIComponent(s.replace(pl, " "));
}

while (match = search.exec(query)) {
    urlParams[decode(match[1])] = decode(match[2]);
}

var d = urlParams["d"];
var s = atob(d);
var options = eval("({" + s + "})");

var fileUpload1 = new Dropzone("#fileUpload1", options);