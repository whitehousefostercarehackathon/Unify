import React, {Component, PropTypes} from 'react';

import {Link} from 'react-router';
import {reduxForm} from 'redux-form';

import InputField from 'components/inputfield';
import Button from 'components/button';

import {login as loginValidation} from './validations';
import { handleLoginSuccess } from 'utils/auth';

import './login-page.scss';

const fields = ['email', 'password'];

@reduxForm({
  form: 'login',
  fields,
  validate: loginValidation
})
class LoginPage extends Component {
  static propTypes = {
    actions: PropTypes.object.isRequired
  };

  constructor(...args) {
    super(...args);

    this.onLoginSubmit = this.onLoginSubmit.bind(this);
  }

  onLoginSubmit({email, password}) {
    return this.props.actions.login(this.props.fields.email.value, this.props.fields.password.value)
      .then(({value}) => {
        handleLoginSuccess(value);
      })
      .catch(({reason: err}) => {
        if (err.status === 401) {
          return Promise.reject({_error: 'Invalid email or password'});
        }

        return Promise.reject();
      });
  }

  render() {

    const {handleSubmit, fields: {email, password}, error} = this.props;
    return (
      <div className="LoginPage">
        <div className="app-logo"></div>
        <div className="LoginPage-header">
          <h1>Login</h1>
          <Link to="/request-account"/>
        </div>
        {error && <div className="AccountLayout-formError">{error}</div>}
        <form onSubmit={handleSubmit(this.onLoginSubmit)}>
          <InputField
            label="Email: "
            {...email}
            type="email"
          />
          <InputField
            label="Password: "
            {...password}
            type="password"
          />

          <div className="AccountLayout-formActions">
            <div className="LoginPage-actions">
              <Link
                to="/register"
                className="LoginPage-register"
              >
                Need an account?
              </Link>
              <Button onClick={this.onLoginSubmit}>SUBMIT</Button>
            </div>
          </div>
        </form>
      </div>
    );
  }
}

export default LoginPage;
