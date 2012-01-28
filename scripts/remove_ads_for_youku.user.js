// ==UserScript==
// @name YOUKU ads Remover
// @namespace https://code.google.com/p/adblock-chinalist/
// @author Gythialy
// @version 1.0.2
// @description Remove YOUKU ads for ChinaList (required Firefox 4+)
// @homepage https://code.google.com/p/adblock-chinalist/
// @updateURL https://adblock-chinalist.googlecode.com/svn/trunk/scripts/remove_ads_for_youku.user.js
// @include http*
// @exclude *localhost*
// @noframes
// ==/UserScript==
/*
 * modified from:http://userscripts.org/scripts/show/109099
 * Thanks NLF
 */
(function() {
	var DEBUG = 0;
	function log(message) {
		if (DEBUG && GM_log) {
			GM_log(message);
		}
	}

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

	function reloadPlugin(elem) {
		var nextSibling = elem.nextSibling;
		var parentNode = elem.parentNode;
		parentNode.removeChild(elem);
		if (nextSibling) {
			parentNode.insertBefore(elem, nextSibling);
		} else {
			parentNode.appendChild(elem);
		}
	}

	function arrayIndexOf(array, elem) {
		try {
			return array.indexOf(elem);
		} catch (e) {
			return -100;
		}
	}

	function init(elem) {
		if (arrayIndexOf(done, elem) != -1)
			return;
		done.push(elem);

		var nodeName = elem.nodeName;
		var needReload;
		if (nodeName === 'OBJECT') {
			var src_data = elem.getAttribute('data');
			log('Process URL (OBJECT) :' + src_data);
			if (/http:\/\/static\.youku\.com\//i.test(src_data)) {
				elem.setAttribute('data', 'http://static.youku.com/v1.0.0212/v/swf/qplayer.swf');
				needReload = true;
			}
		} else if (nodeName === 'EMBED') {
			var src = elem.src;
			if (/http:\/\/player\.youku\.com\/player\.php\//i.test(src)) {
				var re = new RegExp(/\w{13}/g);
				var vid = re.exec(src);
				if (vid) {
					elem.src = 'http://static.youku.com/v1.0.0212/v/swf/qplayer.swf?showAd=0&VideoIDS=' + vid;
					log('Process VIDEO_ID (EMBED) ' + vid);
					needReload = true;
				}
			}
		}

		if (needReload) {
			reloadPlugin(elem);
		}
	}

	var done = [];
	var embeds = x('//embed|//object');
	for ( var i = 0, ii = embeds.snapshotLength; i < ii; i++) {
		init(embeds.snapshotItem(i));
	}

	document.addEventListener('DOMNodeInserted', function(e) {
		var target = e.target;
		if (target.nodeType != 1)
			return;
		var nodeName = target.nodeName;
		if (/OBJECT|EMBED/.test(nodeName)) {
			log('DOMNodeInserted-plugin', target);
			init(target);
		}
		var embeds = x('.//embed|.//object');
		for ( var i = 0, ii = embeds.snapshotLength; i < ii; i++) {
			log('DOMNodeInserted-plugin', embeds.snapshotItem(i));
			init(embeds.snapshotItem(i));
		}
	}, false);
})();