// ==UserScript==
// @author Gythialy
// @version 1.0.6
// @name Remove discuss.com.hk and uwants.com Ads
// @namespace http://code.google.com/p/adblock-chinalist/
// @include http://*.discuss.com.hk/*
// @include http://*.uwants.com/*
// @description Remove discuss.com.hk and uwants.com Ads for ChinaList
// ==/UserScript==
(function() {
	disableHead();
	disableBody();
	removeAds();
	removeFrames();

	function disableHead() {
		var headscript = new Array();
		headscript.push('function indexer2_checker(){');
		headscript.push('}');
		headscript.push('function inner_init(){');
		headscript.push('}');
		var head = document.createElement('script');
		head.innerHTML = headscript.join('\n');
		headscript.length = 0;
		$('/head')[0].appendChild(head);
	}

	function disableBody() {
		var bodyscript = new Array();
		var body = document.createElement('script');
		bodyscript.push('function loadMainAds(zSr){');
		bodyscript.push('}');
		bodyscript.push('function loadThreadAds(zSr){');
		bodyscript.push('}');
		bodyscript.push('function loadRightAds(zSr){');
		bodyscript.push('}');
		body.innerHTML = bodyscript.join('\n');
		bodyscript.length = 0;
		$('/body')[0].appendChild(body);
	}

	function removeAds() {
		var tableads = x('html/body/table[2]/tbody/tr/td[1]/div[1]/table');
		if (tableads.snapshotLength > 0)
			remove(tableads);

		var headad = x('html/body/table[2]/tbody/tr/td[1]/div[1]/div[@align="center"]');
		if (headad.snapshotLength > 0)
			remove(headad);

		var ads = x('//*[@class="ad"]');
		if (ads.snapshotLength > 0)
			remove(ads);
	}
	
	function removeFrames() {
		var iframes = $('/iframe');
		for (var i = 0, length = iframes.length; i < length; i++) {
			var iframe = iframes[i];
			if (iframe && iframe.id != 'posteditor_iframe')
				iframe.parentNode.removeChild(iframe);
		}
	}

	function remove(elm) {
		if (elm.snapshotItem) {
			for (var i = 0; i < elm.snapshotLength; i++) {
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

	function x(xpath, parent, type, result) {
		return document.evaluate(xpath, parent || document, null, type || 7, result);
	}

	function $(select) {
		var name = select.substring(1);
		switch (select.charAt(0)) {
			case '#' :
				return document.getElementById(name);
			case '.' :
				return document.getElementsByClassName(name);
			case '/' :
				return document.getElementsByTagName(name);
			default :
				return document.getElementsByName(select);
		}
	};
})();