let stripe;
let elements;
let paymentElement;

window.stripeInterop = {
    initialize: async (publishableKey, clientSecret) => {
        stripe = Stripe(publishableKey);
        
        const options = {
            clientSecret: clientSecret,
            appearance: {
                theme: 'stripe',
                variables: {
                    colorPrimary: '#0d6efd',
                },
            },
        };

        elements = stripe.elements(options);
        paymentElement = elements.create('payment');
        
        // Asigură-te că elementul container există înainte de a monta
        const paymentElementContainer = document.getElementById('payment-element');
        if (paymentElementContainer) {
            paymentElement.mount('#payment-element');
        } else {
            console.error("Payment element container not found");
        }
    },

    confirmPayment: async (returnUrl) => {
        if (!stripe || !elements) {
            return { error: "Stripe not initialized" };
        }

        const { error } = await stripe.confirmPayment({
            elements,
            confirmParams: {
                return_url: returnUrl,
            },
            redirect: "if_required" // Previne redirectul automat dacă nu e necesar (e.g. card authentication)
        });

        if (error) {
            return { error: error.message };
        } else {
            return { success: true };
        }
    }
};

