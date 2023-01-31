import React, { useCallback, useEffect, useState } from "react";

import { PrimaryButton, ProgressIndicator, Stack, StackItem, TextField, ITextFieldProps } from "@fluentui/react";
import { useBoolean } from '@fluentui/react-hooks';

const styleFunc: ITextFieldProps["styles"] = (props) => ({
    field: { backgroundColor: "#fff" }
})

const InputOrFinished = (props: any) => {
    const { isFinished, isLoading, email, setEmail, onClick, onKeyPress } = props;
    if (isFinished) {

        return <div>Tak, du modtager snart en email med dit login til __EAVFW__ __MainApp__.</div>
    } else {

        return (<>
            <TextField styles={styleFunc} value={email} onChange={(e, v) => setEmail(v)} label="Email"
                disabled={isLoading} placeholder="Enter email ..." onKeyPress={onKeyPress} />
            {isLoading && <ProgressIndicator description="Sending email" />}
            <Stack.Item align="end" styles={{ root: { paddingTop: 16 } }}>
                <PrimaryButton disabled={isLoading} text="Login" onClick={onClick} />

            </Stack.Item>
        </>)
    }
}

const Home = (props: any) => {
    const [loaded, { toggle: toggleLoaded }] = useBoolean(false);
    const [email, setEmail] = useState<string>();
    const [isLoading, { setTrue, setFalse }] = useBoolean(false);
    const [isFinished, { toggle }] = useBoolean(false);
    const _onClick = useCallback(async () => {
        setTrue();
        const redirectUri = `${location.protocol}//${location.host}${new URLSearchParams(location.search).get('ReturnUrl') ?? ''}`;
        location.href = `${process.env.NEXT_PUBLIC_BASE_URL}.auth/login/passwordless?email=${email}&redirectUri=${redirectUri}`;
    }, [email]);

    useEffect(() => {
        const query = new URLSearchParams(location.search);
        if (query.get("email")) {
            toggle();
        }
        toggleLoaded();
    }, [])

    const handleKeyPress = (event: any) => {
        if (event.key === 'Enter') {
            console.log('enter press here! ')
            _onClick();
        }
    }

    console.log("RENDER LOGIN");
    console.log(props);

    if (!loaded)
        return "loading";
    return (
        <div style={{
            width: '100%',
            height: '100%',
            display: 'flex',
            justifyContent: 'center',
            margin: 'auto',
            alignItems: 'center',
            flexDirection: 'column'
        }}>
            <Stack id="LoginBox" >
                {!isFinished && <>Indtast den email, du er registreret med i __EAVFW__ __MainApp__.</>}
                <InputOrFinished isFinished={isFinished} isLoading={isLoading} email={email} setEmail={setEmail}
                    onClick={_onClick} onKeyPress={handleKeyPress} />
            </Stack>
        </div>
    );
}

export default Home;

export function getStaticProps() {
    return {
        props: {
            layout: "AppPickerLayout"
        }
    }
}
