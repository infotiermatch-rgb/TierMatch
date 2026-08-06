import {
  Route,
  Routes,
} from "react-router";

import { MainLayout } from "../layouts/MainLayout";

import { AnimalDetailsPage } from "../pages/AnimalDetailsPage";
import { AnimalsPage } from "../pages/AnimalsPage";
import { ChangePasswordPage } from "../pages/ChangePasswordPage";
import { CreateAdoptionRequestPage } from "../pages/CreateAdoptionRequestPage";
import { EditProfilePage } from "../pages/EditProfilePage";
import { ForgotPasswordPage } from "../pages/ForgotPasswordPage";
import { HomePage } from "../pages/HomePage";
import { LoginPage } from "../pages/LoginPage";
import { MyAdoptionRequestsPage } from "../pages/MyAdoptionRequestsPage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { ProfilePage } from "../pages/ProfilePage";
import { RegisterPage } from "../pages/RegisterPage";
import { ResetPasswordPage } from "../pages/ResetPasswordPage";

import { ShelterAdoptionRequestDetailsPage } from "../pages/shelter/ShelterAdoptionRequestDetailsPage";
import { ShelterAdoptionRequestsPage } from "../pages/shelter/ShelterAdoptionRequestsPage";
import { ShelterDashboardPage } from "../pages/shelter/ShelterDashboardPage";
import { ShelterLoginPage } from "../pages/shelter/ShelterLoginPage";
import { ShelterRegisterPage } from "../pages/shelter/ShelterRegisterPage";

import { ProtectedRoute } from "./ProtectedRoute";
import { ShelterRoute } from "./ShelterRoute";

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<MainLayout />}>
        <Route
          index
          element={<HomePage />}
        />

        <Route
          path="animals"
          element={<AnimalsPage />}
        />

        <Route
          path="animals/:id"
          element={<AnimalDetailsPage />}
        />

        <Route
          path="login"
          element={<LoginPage />}
        />

        <Route
          path="register"
          element={<RegisterPage />}
        />

        <Route
          path="forgot-password"
          element={<ForgotPasswordPage />}
        />

        <Route
          path="reset-password"
          element={<ResetPasswordPage />}
        />

        <Route
          path="shelter/login"
          element={<ShelterLoginPage />}
        />

        <Route
          path="shelter/register"
          element={<ShelterRegisterPage />}
        />

        <Route element={<ProtectedRoute />}>
          <Route
            path="profile"
            element={<ProfilePage />}
          />

          <Route
            path="profile/edit"
            element={<EditProfilePage />}
          />

          <Route
            path="profile/change-password"
            element={<ChangePasswordPage />}
          />

          <Route
            path="my-adoption-requests"
            element={
              <MyAdoptionRequestsPage />
            }
          />

          <Route
            path="animals/:id/adoption-request"
            element={
              <CreateAdoptionRequestPage />
            }
          />
        </Route>

        <Route element={<ShelterRoute />}>
          <Route
            path="shelter"
            element={<ShelterDashboardPage />}
          />

          <Route
            path="shelter/adoption-requests"
            element={
              <ShelterAdoptionRequestsPage />
            }
          />

          <Route
            path="shelter/adoption-requests/:id"
            element={
              <ShelterAdoptionRequestDetailsPage />
            }
          />
        </Route>

        <Route
          path="*"
          element={<NotFoundPage />}
        />
      </Route>
    </Routes>
  );
}