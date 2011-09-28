// ==UserScript==
// @name Qidian ads Remover
// @namespace https://code.google.com/p/adblock-chinalist/
// @author Gythialy
// @version 1.0.1
// @description Remove qidian.com ads for ChinaList
// @homepage https://code.google.com/p/adblock-chinalist/
// @updateURL https://adblock-chinalist.googlecode.com/svn/trunk/scripts/remove_ads_for_qidian.user.js
// @include http://www.qidian.com/*
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
		if (e.target.innerHTML.indexOf('SNDABackPop') > -1) {
			e.stopPropagation();
			e.preventDefault();
			log(e.target.innerHTML + ' stoped.');
		}
	}, true);

	document.addEventListener("DOMNodeInserted", function(e) {
		function x(xpath, parent, type, result) {
			return document.evaluate(xpath, parent || document, null, type || XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, result);
		}

		function remove(elm) {
			if (elm.snapshotItem) {
				for ( var i = 0; i < elm.snapshotLength; i++) {
					remove(elm.snapshotItem(i));
				}
			} else if (elm[0]) {
				while (elm[0]) {
					remove(elm[0]);
				}
			} else {
				elm.parentNode.removeChild(elm);
			}
		}

		var scripts = x('//script');
		for ( var index = 0; index < scripts.snapshotLength; index++) {
			var script = scripts.snapshotItem(index);
			var t = new String(script.innerHTML);
			if (t.indexOf('SNDABackPop') != -1 || t.indexOf('eAddMark') != -1 || t.indexOf('SNDAADAltern') != -1) {
				log('remove ' + t);
				remove(script);
			}
		}
	}, false);
})();