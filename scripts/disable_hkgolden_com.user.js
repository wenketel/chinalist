// ==UserScript==
// @name disable_hkgolden_com
// @namespace https://code.google.com/p/adblock-chinalist/
// @author Gythialy
// @version 1.0.0
// @description disable anti ABP for hkgolden.com
// @homepage https://code.google.com/p/adblock-chinalist/
// @updateURL https://adblock-chinalist.googlecode.com/svn/trunk/scripts/anti_hkgolden.user.js
// @include http://*.hkgolden.com/*
// @grant GM_log
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
		if (e.target.innerHTML.indexOf('blockAdblockUser') > -1) {
			log(e.target.innerHTML);
			e.stopPropagation();
			e.preventDefault();
		}
	}, false);
})();