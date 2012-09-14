// ==UserScript==
// @name Qiyi ads Remover
// @namespace https://code.google.com/p/adblock-chinalist/
// @author Gythialy
// @version 1.0.9
// @description Remove qiyi.com flash ads for ChinaList
// @homepage https://code.google.com/p/adblock-chinalist/
// @updateURL https://adblock-chinalist.googlecode.com/svn/trunk/scripts/remove_ads_for_qiyi.user.js
// @match http://www.iqiyi.com/*
// ==/UserScript==

(function() {
	var DEBUG = 0;
	function log(message) {
		if (DEBUG && GM_log) {
			GM_log(message);
		}
	}

	function getDomain() {
		var d = location.hostname.split(".");
		d = d.slice(d.length - 2);
		return d.join(".");
	}

	function getQCookie(a) {
		var a = a.replace(/([\.\[\]\$])/g, "\\$1");
		return (a = (document.cookie + ";").match(RegExp(a + "=([^;]*)?;", "i"))) ? a.length > 1
				&& a[1] == "deleted" ? "" : decodeURIComponent(a[1]) || "" : "";
	}

	function setQCookie(name, value, expire, path, domain) {
		var g = [];
		g.push(name + "=" + encodeURIComponent(value));
		var t = new Date();
		var c = t.getTime() + expire * 36E5;
		t.setTime(c);
		g.push("expires=" + t.toGMTString())
		g.push("path=" + path);
		g.push("domain=" + domain);

		document.cookie = g.join(";");
	}

	function $(select) {
		var name = select.substring(1);
		switch (select.charAt(0)) {
		case '#':
			return document.getElementById(name);
		case '.':
			return document.getElementsByClassName(name);
		case '/':
			return document.getElementsByTagName(name);
		default:
			return document.getElementsByName(select);
		}
	}

	var a = $("#adBackground"), b = $("#adCloseBtn"), c = getQCookie("TQC001");

	if (a) {
		log(a.className);
		a.className = "";
	}

	log(c);

	if (!c || c !== "true") {
		log(getDomain());
		setQCookie("TQC001", "true", 12, "/", getDomain());
	}
})();
