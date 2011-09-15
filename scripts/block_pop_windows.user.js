// ==UserScript==
// @name blockPopupWindow
// @namespace https://code.google.com/p/adblock-chinalist/
// @author Gythialy
// @version 1.0.0
// @description disable pop windows (only for Scriptish)
// @homepage https://code.google.com/p/adblock-chinalist/
// @updateURL https://adblock-chinalist.googlecode.com/svn/trunk/scripts/block_pop_windows.user.js
// @include http://www.jandown.com/*
// @run-at document-start
// ==/UserScript==
(function() {
	var DEBUG = 0;
	function log(message) {
		if (DEBUG && GM_log) {
			GM_log(message);
		}
	}

	document.addEventListener("beforescriptexecute", function(e) {
		if (e.target.innerHTML.indexOf('window.open') > -1) {
			log(e.target.innerHTML);
			e.stopPropagation();
			e.preventDefault();
		}
	}, false);
})();