// ==UserScript==
// @name blockPopupWindow
// @namespace https://code.google.com/p/adblock-chinalist/
// @description disable pop windows (only for Scriptish)
// @include http://www.jandown.com/*
// @author Gythialy
// @version 1.0.0
// @run-at document-start
// ==/UserScript==
(function() {
	document.addEventListener("beforescriptexecute", function(e) {
				if (e.target.innerHTML.indexOf('window.open') != -1) {
					e.stopPropagation();
					e.preventDefault();
				}
			}, false);
})();