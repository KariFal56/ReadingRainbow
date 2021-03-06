import * as SecureStore from 'expo-secure-store';
import ConfigurationInfo from '../config.json'; 

export async function GetFriends(iUsername) {
    const encodedUsername = encodeURIComponent(iUsername);
    const APIUserService = ConfigurationInfo.APIUserService;
    const fullurl =  APIUserService + `/api/friends/GetFriends/${encodedUsername}`;

    try {
        return SecureStore.getItemAsync('jwt').then(async (token) => {
            const response = await fetch(fullurl,
            {
                headers: {
                                
                    'Authorization': 'Bearer ' + token,
                    'Content-Type': 'application/json; charset=utf-8',
                },
            });
            const friends = await response.json();
            return friends;
        });
    } catch(e) {
        console.log(e);
    } finally {
        console.log('All tasks complete');
    }
}

// function ReturnProfile(info) {
//     console.log(info);
//     var profile = {
//         Email : CheckForNull(info.Email),
//         Name : CheckForNull(info.Name),
//         Portrait : CheckForNull(info.Portrait),
//         Profile : CheckForNull(info.Profile),
//     }
//         return profile;
// }

// function CheckForNull(field) {
//     if (field == null) {
//       return 'No data found';
//     } else {
//       return field;
//     }
// }
