mergeInto(LibraryManager.library, {
  GetBrowserLanguage: function() {
    var lang = navigator.language || (navigator.languages && navigator.languages[0]) || "";
    var bufferSize = lengthBytesUTF8(lang) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(lang, buffer, bufferSize);
    return buffer;
  }
});
