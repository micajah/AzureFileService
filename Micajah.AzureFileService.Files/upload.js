Dropzone.autoDiscover = false;
Dropzone.options.fileUpload1 = false;

var BLOCK_SIZE = 262144, // In bytes. Current value is 256 KB.
    BLOCK_ID_PREFIX = "block-",
    RETRY_TIMEOUT = 5, // In seconds.
    RETRY_NUMBER = 3;

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

var submitUri = atob(urlParams["sas"]);

// TODO: Get the options from query.

var fileUpload1 = new Dropzone("#fileUpload1", {
    url: submitUri,
    acceptedFiles: "image/*",
    method: "PUT",
    dictDefaultMessage: "Drop files here<br />or<br /><input type='button' class='Blue' value='Select Files' />",
    createImageThumbnails: false
});