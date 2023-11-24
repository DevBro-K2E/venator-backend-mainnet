var payLogElement = document.getElementById("pay_log");

async function initPay() {
    var select = document.getElementById("pay_account_select");


    var eth_accounts = await web3.eth.getAccounts();

    eth_accounts.forEach((e, i) => {

        var option = document.createElement("option");

        option.value = e;
        option.text = e;

        select.appendChild(option);
    });



    var btn = document.getElementById("pay_send_btn");

    btn.removeAttribute("disabled");

    btn.addEventListener('click', tryPay);
}

function tryPay() {
    var countInput = document.getElementById("pay_count");

    var paySelect = document.getElementById("pay_account_select");

    var count = countInput.value;

    console.log('tryPay')

    web3.eth.sendTransaction({
        from: paySelect.options[paySelect.selectedIndex].text,
        to: walletAddress,
        value: web3.utils.toWei(count, 'ether'),
        data: web3.utils.toHex(walletIdentity)
    }, (err, transactionId) => {
        if (err) {
            console.log('Payment failed', err)
            payLogElement.innerHTML = 'Payment failed';
        } else {
            console.log('Payment successful', transactionId)
            payLogElement.innerHTML = 'Payment successful. Wait 1min and reload page for update';
        }
    })
}

(async () => {

    if (window.ethereum) {
        window.web3 = new Web3(ethereum);
        try {
            await ethereum.enable();
            await initPay()
        } catch (err) {
            console.error('User denied account access', err)
        }
    } else if (window.web3) {
        window.web3 = new Web3(web3.currentProvider)
        await initPay()
    } else {
        console.error('No Metamask (or other Web3 Provider) installed')
        payLogElement.innerHTML = "Not found Web3 installed providers(Metamask or another)";
    }
})();